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
        [RequestSizeLimit(2 * 1024 * 1024 /* 2MB in bytes */)]
        public async Task<IActionResult> UploadCollection()
        {
            try
            {
                if (!(this.Request.ContentType ?? "").Equals("text/plain", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType, "Only text/plain content is accepted");
                }

                List<AdPlatform> platforms = [];

                Request.EnableBuffering();
                using (var reader = new StreamReader(Request.Body, leaveOpen: true, bufferSize: 1000))
                {
                    while (await reader.ReadLineAsync() is string line)
                    {
                        if (line.Split(':', 2, s_SplitOptionsForColectionUpload) is [string name, string locationsText])
                        {
                            platforms.Add(new AdPlatform(name, locationsText.Split(',', s_SplitOptionsForColectionUpload).AsReadOnly()));
                        }
                    }
                }

                platformsRepository.UploadAdPlatforms(platforms);

                return Ok($"Processed {platforms.Count} lines successfully");
            }
            catch (BadHttpRequestException err) when (err.StatusCode == StatusCodes.Status413PayloadTooLarge)
            {
                return StatusCode(413, $"File exceeds 2MB size limit.");
            }
            catch (Exception err)
            {
                return StatusCode(500, $"Processing error: {err.Message}");
            }
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
