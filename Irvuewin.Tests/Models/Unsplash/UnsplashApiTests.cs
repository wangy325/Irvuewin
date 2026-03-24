#pragma warning disable CS8618
using Irvuewin.Models.Unsplash;
using Irvuewin.Helpers;
using Irvuewin.Helpers.HTTP;

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
            
            // Act
            var actualPhotos = await _httpService.GetPhotosOfChannel(UnsplashDataSet.ChannelId, UnsplashQueryParams.Create());

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