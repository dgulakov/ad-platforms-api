using AdPlatformsApi.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdPlatformsApiTests
{
    public class AdPlatformsRepositoryTests
    {
        private readonly Mock<IAdPlatformsCollection> _mockPlatformsCollection;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly AdPlatformsRepository _repository;
        private readonly CancellationTokenSource _tokenSource = new();

        public AdPlatformsRepositoryTests()
        {
            _mockPlatformsCollection = new Mock<IAdPlatformsCollection>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            //var cacheEntry = Mock.Of<ICacheEntry>();
            //_mockMemoryCache.Setup(cache => cache.CreateEntry(It.IsAny<object>()))
            //    .Returns(cacheEntry);

            _repository = new AdPlatformsRepository(_mockPlatformsCollection.Object, _mockMemoryCache.Object);
        }

        [Fact]
        public async Task SearchPlatformsInLocationAsync_NoCache_PopulatesCache()
        {
            const string location = "/ru/msk";
            var platforms = new ReadOnlyCollection<AdPlatform>(
            [
                new("Platform1", new ReadOnlyCollection<string>([ "/ru" ])),
                new("Platform2", new ReadOnlyCollection<string>([ "/ru/msk" ])),
            ]);

            _mockPlatformsCollection.Setup(c => c.Items).Returns(platforms);

            _mockMemoryCache.Setup(cache => cache.CreateEntry(It.IsAny<object>()))
               .Returns((object key) =>
               {
                   var cacheEntry = new Mock<ICacheEntry>();

                   cacheEntry.SetupProperty(entry => entry.SlidingExpiration)
                        .SetupGet(entry => entry.ExpirationTokens).Returns(new List<IChangeToken>());

                   return cacheEntry.Object;
               });

            _mockMemoryCache.Setup(cache => cache.TryGetValue(
                    It.IsAny<object>(),
                    out It.Ref<object?>.IsAny
                ))
                .Returns(false);


            var result = await _repository.SearchPlatformsInLocationAsync(location);

            Assert.Single(result);
            Assert.Equal("Platform2", result.Single());

            _mockMemoryCache.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task SearchPlatformsInLocationAsync_UsesCache()
        {
            string location = "/ru";
            string cacheKey = $"{nameof(AdPlatformsRepository)}/{nameof(AdPlatformsRepository.SearchPlatformsInLocationAsync)}/{location.GetHashCode()}";
            object? expected = new string[] { "Anything" }.ToImmutableArray();

            _mockMemoryCache.Setup(cache => cache.CreateEntry(It.IsAny<object>()))
               .Returns((object key) => new Mock<ICacheEntry>().Object);

            _mockMemoryCache.Setup(cache => cache.TryGetValue(
                    cacheKey,
                    out expected
                ))
                .Returns(true);

            var result = await _repository.SearchPlatformsInLocationAsync("/ru");

            Assert.Equal(expected, result);
            _mockPlatformsCollection.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UploadAdPlatformsAsync_UpdatesCollectionAndInvalidatesCache()
        {
            var newPlatforms = new List<AdPlatform>
            {
                new("Platform1", new ReadOnlyCollection<string>([ "/ru" ])),
                new("Platform2", new ReadOnlyCollection<string>([ "/ru/msk" ])),
            };

            var initialToken = _repository.GetCacheInvalidationTokenSource();

            await _repository.UploadAdPlatformsAsync(newPlatforms);

            _mockPlatformsCollection.Verify(c => c.Clear(), Times.Once);
            _mockPlatformsCollection.Verify(c => c.AddPlatforms(newPlatforms), Times.Once);

            Assert.NotSame(initialToken, _repository.GetCacheInvalidationTokenSource());
            Assert.True(initialToken.IsCancellationRequested);
        }

        [Fact]
        public async Task GetPlatformsAsync_ReturnsCollectionItems()
        {
            var platforms = new ReadOnlyCollection<AdPlatform>(
            [
                new("Platform1", new ReadOnlyCollection<string>([ "/ru" ])),
                new("Platform2", new ReadOnlyCollection<string>([ "/ru/msk" ])),
            ]);

            _mockPlatformsCollection.Setup(c => c.Items).Returns(platforms);

            var result = await _repository.GetPlatformsAsync();

            Assert.Equal(platforms, result);
        }
    }
}
