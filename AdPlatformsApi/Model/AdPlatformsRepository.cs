using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace AdPlatformsApi.Model
{
    public class AdPlatformsRepository(IAdPlatformsCollection platformsCollection, IMemoryCache cache) : IAdPlatformsRepository
    {
        private static CancellationTokenSource _platformsTokenSource = new();
        private static readonly Lock _platformsTokenLock = new();

        public async Task<IEnumerable<string>> SearchPlatformsInLocationAsync(string location)
        {
            var cacheKey = $"{nameof(AdPlatformsRepository)}/{nameof(SearchPlatformsInLocationAsync)}/{location.GetHashCode()}";

            return await cache.GetOrCreateAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(new CancellationChangeToken(_platformsTokenSource.Token));
                cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(10);

                bool locationIsEmpty = string.IsNullOrWhiteSpace(location.Trim('/'));

                // Perhaps it should be done in parallel because it's more CPU bound neither about pulling data from some persistent storage as database or webservice or distributed cache.
                return await Task.Run(() =>
                    (from item in platformsCollection.Items
                     where locationIsEmpty || item.Locations.Any(platformLocation => platformLocation.StartsWith(location, StringComparison.OrdinalIgnoreCase))
                     select item.PlatformName
                    ).ToImmutableArray());
            });
        }

        public async Task UploadAdPlatformsAsync(IEnumerable<AdPlatform> platforms)
        {
            await Task.Run(() =>
            {
                platformsCollection.Clear();
                platformsCollection.AddPlatforms(platforms);
            });

            lock (_platformsTokenLock)
            {
                _platformsTokenSource.Cancel();
                _platformsTokenSource.Dispose();

                _platformsTokenSource = new CancellationTokenSource();
            }
        }

        public async Task<IEnumerable<AdPlatform>> GetPlatformsAsync() => await Task.FromResult(platformsCollection.Items);

        public CancellationTokenSource GetCacheInvalidationTokenSource() => _platformsTokenSource;
    }
}
