using Irvuewin.Models.Unsplash;
using Irvuewin.Helpers;

namespace Irvuewin.Tests.Models.Unsplash
{
    [TestClass]
    public class UnsplashApiTests
    {
        private UnsplashHttpClientWrapper _httpClientWrapper;
        private UnsplashHttpService _httpService;
        
        [TestInitialize]
        public void TestInitialize()
        {
            // Arrange
            _httpClientWrapper = new UnsplashHttpClientWrapper();
            _httpService = new UnsplashHttpService(_httpClientWrapper);
        }
        
        [TestMethod]
        public async Task GetPhotoById_ReturnUPhoto()
        {
            // Act
            var actualPhoto = await _httpService.GetPhotoInfoById("0RbbLWm6rLk");

            // Assert
            Assert.IsNotNull(actualPhoto);
            Assert.AreEqual(UnsplashDataSet.ExpectedPhoto.Id, actualPhoto.Id);
            Assert.AreEqual(UnsplashDataSet.ExpectedPhoto.Width, actualPhoto.Width);
            Assert.AreEqual(UnsplashDataSet.ExpectedPhoto.Height, actualPhoto.Height);
        }

        [TestMethod]
        public async Task GetPhotos_ReturnList()
        {
            var query = new UnsplashQueryParams
            {
                Page = 1,
                PerPage = 2,
                Orientation = 1
            };
            // Act
            var actualPhotos = await _httpService.GetPhotosOfChannel(UnsplashDataSet.ChannelId, query);

            // Assert
            Assert.IsNotNull(actualPhotos);
            Assert.AreEqual(UnsplashDataSet.ExpectedPhotos.Count, actualPhotos.Count);
            /*for (var i = 0; i < UnsplashDataSet.ExpectedPhotos.Count; i++)
            {
                Assert.AreEqual(UnsplashDataSet.ExpectedPhotos[i].Id, actualPhotos[i].Id);
                Assert.AreEqual(UnsplashDataSet.ExpectedPhotos[i].Slug, actualPhotos[i].Slug);
            }*/
        }
    }
}