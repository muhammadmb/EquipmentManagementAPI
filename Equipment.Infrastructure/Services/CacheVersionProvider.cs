using Application.Interface.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Services
{
    public class CacheVersionProvider : ICacheVersionProvider
    {
        private readonly IDistributedCache _cache;

        public CacheVersionProvider(IDistributedCache cache)
        {
            _cache = cache;
        }

        private static string VersionKey(string scope)
            => $"cache-version:{scope}";

        public async Task<string> GetVersionAsync(string scope)
        {
            var version = await _cache.GetStringAsync(VersionKey(scope));

            if (string.IsNullOrEmpty(version))
            {
                version = Guid.NewGuid().ToString("N");
                await _cache.SetStringAsync(VersionKey(scope), version);
            }

            return version;
        }

        public async Task IncrementAsync(string scope)
        {
            var newVersion = Guid.NewGuid().ToString("N");
            await _cache.SetStringAsync(VersionKey(scope), newVersion);
        }
    }
}
