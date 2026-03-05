using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
using LiteDB;
using Serilog;
using System.IO;

namespace Irvuewin.Helpers
{
    public static class DatabaseManager
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(DatabaseManager));
        private static readonly string DbPath;

        static DatabaseManager()
        {
            var folder = FileUtils.CachedPhotoBaseFolder;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            DbPath = Path.Combine(folder, "irvue.db");
            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>("photos");
                
                // Create indexes for fast filtering in the future
                photos.EnsureIndex(x => x.Id, true);
                photos.EnsureIndex(x => x.Width);
                photos.EnsureIndex(x => x.Height);
                photos.EnsureIndex(x => x.User.Name);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize LiteDB");
            }
        }

        public static void UpsertPhotos(string channelId, IEnumerable<UnsplashPhoto> newPhotos)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>("photos");

                foreach (var photo in newPhotos)
                {
                    // Existing check to append ChannelId without destructive overwrite
                    var existing = photos.FindById(photo.Id);
                    if (existing != null)
                    {
                        if (existing.ChannelIds != null)
                        {
                            photo.ChannelIds = existing.ChannelIds;
                        }
                    }
                    else
                    {
                        photo.ChannelIds = new List<string>();
                    }

                    if (!photo.ChannelIds.Contains(channelId))
                    {
                        photo.ChannelIds.Add(channelId);
                    }
                }

                photos.Upsert(newPhotos);
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
                var photos = db.GetCollection<UnsplashPhoto>("photos");
                
                // LiteDB index optimized array matching
                return photos.Find(x => x.ChannelIds.Contains(channelId)).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load photos from LiteDB");
                return new List<UnsplashPhoto>();
            }
        }

        public static void RemoveChannelPhotos(string channelId)
        {
            try
            {
                using var db = new LiteDatabase(DbPath);
                var photos = db.GetCollection<UnsplashPhoto>("photos");

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
