using Irvuewin.Helpers.DB;
using Irvuewin.Helpers.Events;
using Irvuewin.Helpers.HTTP;
using Irvuewin.Models.Unsplash;
using Serilog;

namespace Irvuewin.Helpers
{
    public class UnsplashSyncWorker
    {
        private static readonly ILogger Logger = Log.ForContext<UnsplashSyncWorker>();
        private readonly UnsplashHttpService _apiService;
        private Timer? _backgroundTimer;

        private static UnsplashSyncWorker? _instance;
        private static readonly Lock Lock = new();

        public static UnsplashSyncWorker Instance => _instance ?? throw new InvalidOperationException("UnsplashSyncWorker is not initialized.");

        public static void Initialize(UnsplashHttpService apiService)
        {
            if (_instance != null) return;
            lock (Lock)
            {
                _instance ??= new UnsplashSyncWorker(apiService);
            }
            Logger.Information("UnsplashSyncWorker initialized.");
        }

        private UnsplashSyncWorker(UnsplashHttpService apiService)
        {
            _apiService = apiService;
            EventBus.ForceSyncRequested += OnForceSyncRequested;

            // 启动定时巡检，间隔设定为4小时
            var syncInterval = TimeSpan.FromHours(4);
            _backgroundTimer = new Timer(DoBackgroundSync, null, syncInterval, syncInterval);
        }

        private async void OnForceSyncRequested(string channelId)
        {
            try
            {
                await SyncChannelAsync(channelId, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Force sync requested failed for channel {channelId}", channelId);
            }
            finally
            {
                // Must ensure UI stops loading status
                EventBus.PublishChannelSyncCompleted(channelId);
            }
        }

        private async void DoBackgroundSync(object? state)
        {
            Logger.Information("Background silent sync started...");
            try
            {
                var channels = DataBaseService.LoadChannels();
                if (channels == null || channels.Count == 0) return;

                foreach (var channel in channels)
                {
                    await SyncChannelAsync(channel.Id, false);
                    await Task.Delay(2000); // 间隔请求，避免频率过高触发API限制
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Background silent sync failed.");
            }
        }

        /// <summary>
        /// Sync photos from Unsplash page 1 for the specific channel.
        /// </summary>
        /// <param name="channelId">Target channel</param>
        /// <param name="forceClearData">If true, clears existing cached photos for this channel or re-assigns sequence. Often false in silent worker.</param>
        private async Task SyncChannelAsync(string channelId, bool forceClearData)
        {
            var channel = DataBaseService.GetChannel(channelId);
            if (channel == null) return;

            var query = UnsplashQueryParams.Create();
            
            var photos = await _apiService.GetPhotosOfChannel(channelId, query);
            if (photos == null || photos.Count == 0)
            {
                // 如果是没拿到照片或者照片空，标记
                channel.AllPhotosLoaded = true;
                await DataBaseService.UpdateChannel(channel);
                return;
            }

            // Upsert photos without disrupting DB too much
            await DataBaseService.CachePhotos(channelId, photos);

            // Fetch successfully -> update sequence and flag
            channel.AllPhotosLoaded = false;
            // 对于强制刷新，确保 Shard 置为 1 (与之前 ChannelsViewModel 修改对齐)
            if (forceClearData)
            {
                channel.Shard = 1;
                channel.Sequence = 1;
            }
            else
            {
                // 如果是后台巡检拿到了新图，由于我们是拉取的最新的第一页，最好也覆盖一下数据源结构
                // 但考虑到用户当前正在看的内容不应被打断，静默同步只负责入库即可。
            }

            await DataBaseService.UpdateChannel(channel);
            Logger.Information("Synced channel {channelId} from API page 1. Upserted {c} photos.", channelId, photos.Count);
        }
    }
}
