using FastCache;
using FastCache.Collections;


namespace Irvuewin.Helpers.DB
{
    /// <summary>
    /// In-memory cache helper
    /// </summary>
    public static class FastCacheManager
    {
        /// <summary>
        /// Default Timespan: 7 days
        /// </summary>
        private static readonly TimeSpan Expiration = TimeSpan.FromDays(7);


        /// <summary>
        /// Saves a value to the cache with an optional expiration.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="expiration">The expiration time. Defaults to 1 day if not specified.</param>
        /// <returns>The value cached</returns>
        public static T Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var expiry = expiration ?? Expiration;
            return Cached<T>.Save(key, value, expiry);
        }

        /// <summary>
        /// Saves a value to the cache with an optional expiration by using 2 Keys.
        /// </summary>
        /// <param name="key1">cache key 1</param>
        /// <param name="key2">cache key 2</param>
        /// <param name="value">cache value</param>
        /// <param name="expiration">The expiration time.</param>
        /// <returns>The value cached</returns>
        public static T Set<T>(string key1, string key2, T value, TimeSpan? expiration = null)
        {
            var expiry = expiration ?? Expiration;
            return Cached<T>.Save(key1, key2, value, expiry);
        }

        /// <summary>
        /// Save a range of k-v in one operation.
        /// </summary>
        /// <param name="range">k-v pair list</param>
        /// <param name="expiration">The expiration time.</param>
        /// <typeparam name="TS">Key type</typeparam>
        /// <typeparam name="T"></typeparam>
        public static void SetRange<TS, T>(IEnumerable<(TS, T)> range, TimeSpan? expiration = null) where TS : notnull
        {
            var expiry = expiration ?? Expiration;
            CachedRange<T>.Save(range, expiry);
        }

        /// <summary>
        /// Retrieves a value from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached value, or default(T) if not found.</returns>
        public static T? Get<T>(string key)
        {
            return Cached<T>.TryGet(key, out var cachedValue) ? cachedValue.Value : default;
        }

        public static T? Get<T>(string key1, string key2)
        {
            return Cached<T>.TryGet(key1, key2, out var cachedValue) ? cachedValue.Value : default;
        }


        /// <summary>
        /// Tries to retrieve a value from the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if method get value of given key</returns>
        public static bool TryGet<T>(string key, out T? value)
        {
            if (Cached<T>.TryGet(key, out var cached))
            {
                value = cached.Value;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGet<T>(string key1, string key2, out T? value)
        {
            if (Cached<T>.TryGet(key1, key2, out var cached))
            {
                value = cached.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Remove cache <br/>
        /// Can only remove item with multiple keys...
        /// </summary>
        /// <param name="key1">key1</param>
        /// <param name="key2">key2</param>
        /// <typeparam name="T">value type</typeparam>
        public static void Remove<T>(string key1, string key2)
        {
            var list = new List<(string, string)> { (key1, key2) };
            CachedRange<T>.Remove(list);
        }

        public static bool Exists<T>(string key)
        {
            return TryGet<T>(key, out _);
        }

        public static bool Exists<T>(string key1, string key2)
        {
            return TryGet<T>(key1, key2, out _);
        }
    }
}