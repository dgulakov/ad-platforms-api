using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Runtime.CompilerServices;

namespace AdPlatformsApi.Model
{
    public class AdPlatformsRepository(IAdPlatformsCollection platformsCollection, IMemoryCache cache, ILogger<AdPlatformsRepository> logger)
    {
        private static CancellationTokenSource _platformsTokenSource = new();

        public IEnumerable<string> SearchPlatformsInLocation(string location)
        {
            var cacheKey = $"{nameof(AdPlatformsRepository)}/{nameof(SearchPlatformsInLocation)}/{location.GetHashCode()}";

            return cache.GetOrCreate(cacheKey, cacheEntry =>
            {
                cacheEntry.AddExpirationToken(new CancellationChangeToken(_platformsTokenSource.Token));
                cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(10);

                bool locationIsEmpty = string.IsNullOrWhiteSpace(location.Trim('/'));

                return (from item in platformsCollection.Items
                       where locationIsEmpty || item.Locations.Any(platformLocation => platformLocation.StartsWith(location, StringComparison.OrdinalIgnoreCase))
                       select item.PlatformName).ToList();
            }) ?? [];
        }

        public void UploadAdPlatforms(IEnumerable<AdPlatform> platforms)
        {
            platformsCollection.Clear();
            platformsCollection.AddPlatforms(platforms);

            _platformsTokenSource.Cancel();
            _platformsTokenSource.Dispose();

            _platformsTokenSource = new CancellationTokenSource();
        }

        public IEnumerable<AdPlatform> GetPlatforms()
        {
            return platformsCollection.Items;
        }
    }
}
