using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
using LiteDB;
using Serilog;
using System.IO;
using static Irvuewin.Helpers.IAppConst;

namespace Irvuewin.Helpers.DB
{
    public static class DatabaseManager
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(DatabaseManager));
        private static readonly string DbPath;

        static DatabaseManager()
        {
            var folder = FileUtils.CachedResourceFolder;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var dbFile = Path.Combine(folder, $"{AppName.ToLower()}.db");
            DbPath = $"Filename={dbFile};Connection=shared";
            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            using var db = new LiteDatabase(DbPath);
            InitChannelDataBase(db);
            InitPhotoDataBase(db);
        }

        private static void InitChannelDataBase(LiteDatabase db)
        {
            try
            {
                var channels = db.GetCollection<UnsplashChannel>(DbChannelCollection);
                channels.EnsureIndex(x => x.Id, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize channel LiteDB");
            }
        }

        private static void InitPhotoDataBase(LiteDatabase db)
        {
            try
            {
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);

                // Create indexes for fast filtering in the future
                photos.EnsureIndex(x => x.Id, true);
                photos.EnsureIndex(x => x.Width);
                photos.EnsureIndex(x => x.Height);
                photos.EnsureIndex(x => x.User.Name);
                photos.EnsureIndex("ChannelIds" ,"$.ChannelIds[*]");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize photos LiteDB");
            }
        }

        public static void UpsertChannel(UnsplashChannel channel)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var channels = db.GetCollection<UnsplashChannel>(DbChannelCollection);
                channels.Upsert(channel);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to upsert channel");
            }
        }

        public static void UpsertChannel(IEnumerable<UnsplashChannel> channels)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var cc = db.GetCollection<UnsplashChannel>(DbChannelCollection);
                cc.Upsert(channels);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to upsert channels");
            }
        }

        public static UnsplashChannel? GetChannelById(string channelId)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var channels = db.GetCollection<UnsplashChannel>(DbChannelCollection);
                return channels.FindById(channelId);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get channel");
            }

            return null;
        }

        public static List<UnsplashChannel>? GetAllChannels()
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var channels = db.GetCollection<UnsplashChannel>(DbChannelCollection);
                return channels.FindAll().ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get channels");
            }

            return null;
        }

        public static void RemoveChannel(string channelId)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var channels = db.GetCollection<UnsplashChannel>(DbChannelCollection);
                channels.Delete(channelId);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to remove channel");
            }
        }

        public static void UpsertPhotos(string channelId, IEnumerable<UnsplashPhoto> newPhotos)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var oldPhotos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);
                
                var unsplashPhotos = newPhotos.ToList();
                
                var currentTick = DateTimeOffset.UtcNow.Ticks;
                var offset = 0;
                foreach (var photo in unsplashPhotos)
                {
                    photo.LocalSortIndex = currentTick + offset++;
                    // Existing check to append ChannelId without destructive overwrite
                    var existing = oldPhotos.FindById(photo.Id);
                    if (existing != null)
                    {
                        if (existing.ChannelIds.Count > 0)
                        {
                            photo.ChannelIds = existing.ChannelIds;
                        }
                    }
                    else
                    {
                        photo.ChannelIds = [];
                    }

                    if (!photo.ChannelIds.Contains(channelId))
                    {
                        photo.ChannelIds.Add(channelId);
                    }
                }

                oldPhotos.Upsert(unsplashPhotos);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to upsert photos to LiteDB");
            }
        }

        public static List<UnsplashPhoto> GetPhotos(string channelId)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);

                // LiteDB index optimized array matching
                return photos.Find(x => x.ChannelIds.Contains(channelId)).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load photos from LiteDB");
                return [];
            }
        }

        public static List<UnsplashPhoto> GetPhotos(string channelId, int page, int pageSize)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);

                var skipCount = (page - 1) * pageSize;
                // 注意：因为分页依赖于排序，通常我们需要定义一个明确的排序规则（比如按添加到数据库的时间降序）
                return photos.Query()
                    .Where(x=> x.ChannelIds.Contains(channelId))
                    .OrderBy(x=>x.LocalSortIndex)
                    .Skip(skipCount)
                    .Limit(pageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load paginated photos from LiteDB");
                return [];
            }
        }

        public static int CountPhotos(string channelId)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);
                return photos.Count(x => x.ChannelIds.Contains(channelId));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to count photos from LiteDB");
                return 0;
            }
        }

        public static void RemoveChannelPhotos(string channelId)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);

                var relatedPhotos = photos.Find(x => x.ChannelIds.Contains(channelId)).ToList();
                foreach (var photo in relatedPhotos)
                {
                    photo.ChannelIds.Remove(channelId);
                    if (photo.ChannelIds.Count == 0)
                    {
                        photos.Delete(photo.Id);
                    }
                    else
                    {
                        photos.Update(photo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to clear channel photos from LiteDB");
            }
        }
    }
}