using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
using LiteDB;
using Serilog;
using System.IO;
using Irvuewin.Helpers.AOP;
using static Irvuewin.Helpers.IAppConst;

namespace Irvuewin.Helpers.DB
{
    internal static class DatabaseManager
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
                photos.EnsureIndex(x => x.IsLiked);
                photos.EnsureIndex(x => x.LikedAt);
                photos.EnsureIndex("ChannelIds", "$.ChannelIds[*]");
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

        public static UnsplashPhoto? GetPhotoById(string photoId)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);
                return photos.FindById(photoId);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get photo by id");
                return null;
            }
        }

        public static void UpsertPhoto(UnsplashPhoto photo)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);
                photos.Upsert(photo);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Update photo error.");
            }
        }

        public static void UpdatePhotoLikedStatus(string photoId, bool isLiked)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);
                var photo = photos.FindById(photoId);
                if (photo == null) return;
                photo.IsLiked = isLiked;
                photo.LikedAt = isLiked ? DateTimeOffset.UtcNow.Ticks : 0;
                photos.Update(photo);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to update photo liked status");
            }
        }

        public static void BlockAuthor(string username)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);
                var userPhotos = photos.Find(x => x.User.Username == username).ToList();
                foreach (var p in userPhotos)
                {
                    p.IsBlocked = true;
                }

                photos.Update(userPhotos);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to block author in LiteDB");
            }
        }

        public static void UnblockAuthor(string username)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);
                var userPhotos = photos.Find(x => x.User.Username == username).ToList();
                foreach (var p in userPhotos)
                {
                    p.IsBlocked = false;
                }

                photos.Update(userPhotos);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to unblock author in LiteDB");
            }
        }

        [FilterByBlockList]
        [SmartFilter]
        [FilterBySize]
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

        /// <summary>
        /// universe wallpaper orientation filter 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static ILiteQueryable<UnsplashPhoto> ApplyOrientationFilter(ILiteQueryable<UnsplashPhoto> query)
        {
            return Properties.Settings.Default.WallpaperOrientation switch
            {
                // landscape
                0 => query.Where(x => x.IsVertical == false),
                // portrait
                1 => query.Where(x => x.IsVertical == true),
                // squarish
                // 2 => query.Where(x => x.Width == x.Height),
                _ => query
            };
        }

        public static List<UnsplashPhoto> GetPhotosByOffset(string channelId, int skipCount, int takeCount)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);

                var query = photos.Query();
                if (channelId == LikesChannelId)
                {
                    query = query.Where(x => x.IsFiltered == false && x.IsLiked);
                    query = ApplyOrientationFilter(query);
                    return query
                        .OrderByDescending(x => x.LikedAt)
                        .Skip(skipCount)
                        .Limit(takeCount)
                        .ToList();
                }

                query = query.Where(x => x.IsFiltered == false && x.ChannelIds.Contains(channelId));
                query = ApplyOrientationFilter(query);

                return query
                    .OrderBy(x => x.LocalSortIndex)
                    .Skip(skipCount)
                    .Limit(takeCount)
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load offset photos from LiteDB");
                return [];
            }
        }

        public static UnsplashPhoto? GetPhotoBySequence(string channelId, int skip)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);

                var query = photos.Query();
                if (channelId == LikesChannelId)
                {
                    query = query.Where(x => x.IsFiltered == false && x.IsLiked);
                    query = ApplyOrientationFilter(query);
                    return query
                        .OrderByDescending(x => x.LikedAt)
                        .Skip(skip - 1)
                        .Limit(1)
                        .Single();
                }

                query = query.Where(x => x.IsFiltered == false && x.ChannelIds.Contains(channelId));
                query = ApplyOrientationFilter(query);

                return query
                    .OrderBy(x => x.LocalSortIndex)
                    .Skip(skip - 1)
                    .Limit(1)
                    .Single();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load specify photo from LiteDB");
                return null;
            }
        }

        /// <summary>
        /// count photos of specify channel
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="exclude">exclude filtered photos</param>
        /// <returns></returns>
        public static int CountPhotos(string channelId, bool exclude = false)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);
                // return photos.Count(x => x.ChannelIds.Contains(channelId));
                var query = photos.Query();

                if (channelId == LikesChannelId)
                {
                    query = exclude
                        ? query.Where(x => x.IsFiltered == false && x.IsLiked)
                        : query.Where(x => x.IsLiked);
                }
                else
                {
                    query = exclude
                        ? query.Where(x => x.IsFiltered == false && x.ChannelIds.Contains(channelId))
                        : query.Where(x => x.ChannelIds.Contains(channelId));
                }

                query = ApplyOrientationFilter(query);

                return query.Count();
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
                    if (photo.ChannelIds.Count == 0 && !photo.IsLiked)
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

        public static List<UnsplashPhoto> GetRandomLikedPhotos(int count)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>(DbPhotoCollection);
                var query = photos.Query().Where(x => x.IsFiltered == false && x.IsLiked);
                query = ApplyOrientationFilter(query);
                var likedPhotos = query.ToList();
                if (likedPhotos.Count == 0) return [];

                var random = new Random();
                return likedPhotos.OrderBy(x => random.Next()).Take(count).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get random liked photos");
                return [];
            }
        }
    }
}