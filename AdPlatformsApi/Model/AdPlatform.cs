using System.Collections.ObjectModel;

namespace AdPlatformsApi.Model;

public record AdPlatform(string PlatformName, ReadOnlyCollection<string> Locations);