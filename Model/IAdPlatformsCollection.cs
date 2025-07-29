using System.Collections.ObjectModel;

namespace AdPlatformsApi.Model
{
    public interface IAdPlatformsCollection
    {
        void Clear();

        public ReadOnlyCollection<AdPlatform> Items { get; }

        public void AddPlatform(AdPlatform platform);

        public void AddPlatforms(IEnumerable<AdPlatform> platforms);
    }
}
