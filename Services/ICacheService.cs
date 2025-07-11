using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace FEENALOoFINALE.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        Task ClearByPatternAsync(string pattern);
    }

    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly HashSet<string> _cacheKeys;
        private readonly object _lock = new object();

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
            _cacheKeys = new HashSet<string>();
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            var value = _cache.Get<T>(key);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            var options = new MemoryCacheEntryOptions();
            
            if (expiry.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiry;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            }

            options.RegisterPostEvictionCallback((k, v, reason, state) =>
            {
                lock (_lock)
                {
                    _cacheKeys.Remove(k.ToString()!);
                }
            });

            _cache.Set(key, value, options);
            
            lock (_lock)
            {
                _cacheKeys.Add(key);
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            lock (_lock)
            {
                _cacheKeys.Remove(key);
            }
            return Task.CompletedTask;
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            lock (_lock)
            {
                var keysToRemove = _cacheKeys.Where(k => k.Contains(pattern)).ToList();
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _cacheKeys.Remove(key);
                }
            }
            return Task.CompletedTask;
        }

        public Task ClearByPatternAsync(string pattern)
        {
            // Same implementation as RemoveByPatternAsync for compatibility
            return RemoveByPatternAsync(pattern);
        }
    }
}
