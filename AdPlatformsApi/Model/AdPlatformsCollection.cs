using System.Collections.ObjectModel;

namespace AdPlatformsApi.Model
{
    public class AdPlatformsCollection : IAdPlatformsCollection
    {
        private readonly List<AdPlatform> _platforms =
        [
            new("Яндекс.Директ", new ReadOnlyCollection<string>([ "/ru" ])),
            new("Ревдинский рабочий", new ReadOnlyCollection<string>(["/ru/svrd/revda", "/ru/svrd/pervik"])),
            new("Газета уральских москвичей", new ReadOnlyCollection<string>(["/ru/msk", "/ru/permobl", "/ru/chelobl"])),
            new("Крутая реклама", new ReadOnlyCollection<string>(["/ru/svrd"]))
        ];

        private readonly Lock _lock = new();

        public ReadOnlyCollection<AdPlatform> Items => _platforms.AsReadOnly();

        public void AddPlatform(AdPlatform platform)
        {
            lock (_lock)
            {
                _platforms.Add(platform);
            }
        }

        public void AddPlatforms(IEnumerable<AdPlatform> platforms)
        {
            lock (_lock)
            {
                _platforms.AddRange(platforms);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _platforms.Clear();
            }
        }
    }
}
