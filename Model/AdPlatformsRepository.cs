namespace AdPlatformsApi.Model
{
    public class AdPlatformsRepository(IAdPlatformsCollection platformsCollection)
    {
        public IEnumerable<string> SearchPlatformsInLocation(string location)
        {
            bool locationIsEmpty = string.IsNullOrWhiteSpace(location.Trim('/'));

            return from item in platformsCollection.Items
                   where locationIsEmpty || item.Locations.Any(platformLocation => platformLocation.StartsWith(location, StringComparison.OrdinalIgnoreCase))
                   select item.PlatformName;
        }

        public void UploadAdPlatforms(IEnumerable<AdPlatform> platforms)
        {
            platformsCollection.Clear();
            platformsCollection.AddPlatforms(platforms);
        }

        public IEnumerable<AdPlatform> GetPlatforms()
        {
            return platformsCollection.Items;
        }
    }
}
