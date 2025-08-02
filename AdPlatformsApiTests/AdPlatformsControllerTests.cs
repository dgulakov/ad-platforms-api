using AdPlatformsApi.Controllers;
using AdPlatformsApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AdPlatformsApiTests
{
    public class AdPlatformsControllerTests
    {
        private readonly Mock<IAdPlatformsRepository> _platformsRepository;
        private readonly AdPlatformsController _controller;

        public AdPlatformsControllerTests()
        {
            _platformsRepository = new Mock<IAdPlatformsRepository>();

            _controller = new AdPlatformsController(_platformsRepository.Object);
        }

        [Fact]
        public async Task GetAdPlatforms_OkResult()
        {
            var adPlatforms = new List<AdPlatform>()
            {
                new ("Test Ad Platform", new ReadOnlyCollection<string>(["/ru"])),
                new ("Mega Platform for your Business", new ReadOnlyCollection<string>(["/ru", "/by", "/kz"]))
            };

            _platformsRepository.Setup(repo => repo.GetPlatformsAsync())
                .ReturnsAsync(adPlatforms);

            var result = await _controller.Get();

            Assert.Equal(adPlatforms, result);
        }

        [Theory]
        [InlineData("/", new[] { "Platform1", "Platform2" })]
        [InlineData("/ru", new[] { "Platform1", "Platform2" })]
        [InlineData("/ru/msk", new[] { "Platform1" })]
        [InlineData("/ru/vrn", new[] { "Platform2" })]
        public async Task SearchAdPlatforms_FilteredResults(string? location, string[] expected)
        {
            _platformsRepository.Setup(repo => repo.SearchPlatformsInLocationAsync(AdPlatformsController.NormalizeLocationString(location)))
                .ReturnsAsync(expected);

            var result = await _controller.SearchPlatforms(location);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task UploadCollection_ValidFile_ProcessesSuccessfully()
        {
            _platformsRepository.Setup(repo => repo.UploadAdPlatformsAsync(It.IsAny<IEnumerable<AdPlatform>>()))
                .Returns(Task.CompletedTask);

            var content = """
                Яндекс.Директ:/ru
                Комсомольская Правда:/ru/msk
                Ударники Труда:/ru/vrn/komintern
                Крутая реклама:/ru/svrd,/ru/krd
                """;

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            var context = new DefaultHttpContext
            {
                Request =
                {
                    Body = stream,
                    ContentType = "text/plain",
                    ContentLength = stream.Length
                }
            };

            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            var result = await _controller.UploadCollection();

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Processed 4 lines successfully", ((OkObjectResult)result).Value);

            _platformsRepository.Verify(r => r.UploadAdPlatformsAsync(It.IsAny<IEnumerable<AdPlatform>>()), Times.Once);
        }

        [Fact]
        public async Task UploadCollection_InvalidContentType_Returns415()
        {
            var content = """
                [
                    {
                        "name": "Яндекс.Директ",
                        "locations": [ "/ru" ]
                    },
                    {
                        "name": "Комсомольская Правда",
                        "locations": [ "/ru/msk" ]
                    },
                    {
                        "name": "Ударники Труда",
                        "locations": [ "/ru/vrn/komintern" ]
                    },
                    {
                        "name": "Крутая реклама",
                        "locations": [ "/ru/svrd", "/ru/krd" ]
                    }
                ]
                """;

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            var context = new DefaultHttpContext
            {
                Request =
                {
                    Body = stream,
                    ContentType = "application/json",
                    ContentLength = stream.Length
                }
            };

            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            var result = await _controller.UploadCollection();

            Assert.IsType<ObjectResult>(result);
            Assert.Equal(415, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task UploadCollection_PartiallyValidFile_ProcessesSuccessfullyOnlyValidRows()
        {
            _platformsRepository.Setup(repo => repo.UploadAdPlatformsAsync(It.IsAny<IEnumerable<AdPlatform>>()))
                .Returns(Task.CompletedTask);

            var content = """
                Яндекс.Директ:/ru
                Комсомольская Правда /ru/msk
                Ударники Труда: ru\vrn\ asasd
                """;

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            var context = new DefaultHttpContext
            {
                Request =
                {
                    Body = stream,
                    ContentType = "text/plain",
                    ContentLength = stream.Length
                }
            };

            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            var result = await _controller.UploadCollection();

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Processed 1 lines successfully", ((OkObjectResult)result).Value);

            _platformsRepository.Verify(r => r.UploadAdPlatformsAsync(It.IsAny<IEnumerable<AdPlatform>>()), Times.Once);
        }

        [Theory]
        [InlineData(null, "/")]
        [InlineData("", "/")]
        [InlineData("//", "/")]
        [InlineData("ru", "/ru")]
        [InlineData("/ru", "/ru")]
        [InlineData("ru/", "/ru")]
        [InlineData("ru/msk/", "/ru/msk")]
        public void NormalizeLocationString_ReturnsCorrectValue(string? input, string expected)
        {
            var result = AdPlatformsController.NormalizeLocationString(input);

            Assert.Equal(expected, result);
        }
    }
}
