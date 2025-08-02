
namespace AdPlatformsApi.Model
{
    public interface IAdPlatformsRepository
    {
        Task<IEnumerable<AdPlatform>> GetPlatformsAsync();

        Task<IEnumerable<string>> SearchPlatformsInLocationAsync(string location);

        Task UploadAdPlatformsAsync(IEnumerable<AdPlatform> platforms);

        CancellationTokenSource GetCacheInvalidationTokenSource();
    }
}