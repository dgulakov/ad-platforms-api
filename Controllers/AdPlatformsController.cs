using AdPlatformsApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace AdPlatformsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdPlatformsController(AdPlatformsRepository platformsRepository) : ControllerBase
    {
        private static readonly StringSplitOptions s_SplitOptionsForColectionUpload = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

        [HttpGet]
        public IEnumerable<AdPlatform> Get()
        {
            return platformsRepository.GetPlatforms();
        }

        [HttpGet("search/{*location}")]
        public IEnumerable<string> SearchPlatforms(string? location)
        {
            location = NormalizeLocationString(location);

            return platformsRepository.SearchPlatformsInLocation(location);
        }

        [HttpPut]
        public async Task UploadCollection([FromBody] string collectionAsText)
        {
            List<AdPlatform> platforms = [];

            using (var sr = new StringReader(collectionAsText))
            {
                while (await sr.ReadLineAsync() is string platformLine)
                {
                    if (platformLine.Split(':', 2, s_SplitOptionsForColectionUpload) is [string name, string locationsText])
                    {
                        platforms.Add(new AdPlatform(name, locationsText.Split(',', s_SplitOptionsForColectionUpload).AsReadOnly()));
                    }
                }
            }

            platformsRepository.UploadAdPlatforms(platforms);
        }

        private string NormalizeLocationString(string? location)
        {
            location ??= "";

            if (!location.StartsWith('/'))
            {
                location = $"/{location}";
            }

            return location.TrimEnd('/');
        }
    }
}
