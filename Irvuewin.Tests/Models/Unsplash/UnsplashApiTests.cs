using Irvuewin.Models.Unsplash;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Irvuewin.Helpers;

namespace Irvuewin.Tests.Models.Unsplash
{

    [TestClass]
    public class UnsplashApiTests
    {
        private Mock<IHttpClient> _mockHttpClient;
        private UnsplashHttpService _helper;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockHttpClient = new Mock<IHttpClient>();
            // _mockHttpClient.Setup(x => x.Client());
            _helper = new UnsplashHttpService(_mockHttpClient.Object);
            
        }
        [TestMethod]
        public async Task GetAsyncPhotoById_ReturnsDeserializedUnsplashPhoto()
        { 
            // Arrange
            var photoId = "0RbbLWm6rLk";
            var apiUrl = $"https://api.unsplash.com/photos/{photoId}";
            var expectedPhoto = new UnsplashPhoto()
            {
                Id = photoId,
                Width = 5944,
                Height = 3949,
                Slug = "woman-in-white-tank-top-wearing-black-sunglasses-0RbbLWm6rLk"
            };
            var mockedResponseContent = JsonConvert.SerializeObject(expectedPhoto, JsonHelper.Settings);
            var mockedHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(mockedResponseContent, Encoding.UTF8, "application/json")
            };
            _mockHttpClient.Setup(x => x.GetAsync(apiUrl)).ReturnsAsync(mockedHttpResponse);
            
            // Act
            var actualPhoto = await _helper.GetPhotoInfoById(photoId);
            
            // Assert
            Assert.IsNotNull(actualPhoto);
            Assert.AreEqual(expectedPhoto.Id, actualPhoto.Id);
            Assert.AreEqual(expectedPhoto.Width, actualPhoto.Width);
            Assert.AreEqual(expectedPhoto.Height, actualPhoto.Height);
            Assert.AreEqual(expectedPhoto.Slug, actualPhoto.Slug);
            
            _mockHttpClient.Verify(x => x.GetAsync(apiUrl), Times.Once());
            
        }
        
        [TestMethod]
        public async Task GetAsyncPhotoById_InvalidJsonResponse_ReturnsDefault()
        {
            // Arrange
            var photoId = "test_photo_id_bad_json";
            var fakeInvalidJsonResponse = "this is not a valid json"; 

            var fakeHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fakeInvalidJsonResponse, Encoding.UTF8, "application/json")
            };

            _mockHttpClient
                .Setup(client => client.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(fakeHttpResponse);

            // Act & Assert
            var result = await _helper.GetPhotoInfoById(photoId);
            
            Assert.IsNull(result);
        }
        
        [TestMethod]
        public async Task GetAsync_HttpRequestError_ReturnsDefault()
        {
            // Arrange
            _mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Simulated network error"));
            
            // Act
            var result = await _helper.GetAsync<UnsplashPhoto>("photos/random");

            // Assert
            Assert.IsNull(result);
        }
        
        [TestMethod]
        public async Task GetPhotoById_ReturnUPhoto()
        {
            // Arrange
            var expectedPhoto = new UnsplashPhoto()
            {
                Id = "0RbbLWm6rLk",
                Width = 5944,
                Height = 3949,
                Slug = "woman-in-white-tank-top-wearing-black-sunglasses-0RbbLWm6rLk"
            };

            var wrapper = new HttpClientWrapper();
           

            var helper = new UnsplashHttpService(wrapper);

            // Act
            var actualPhoto = await helper.GetPhotoInfoById("0RbbLWm6rLk");

            // Assert
            Assert.IsNotNull(actualPhoto);
            Assert.AreEqual(expectedPhoto.Id, actualPhoto.Id);
            Assert.AreEqual(expectedPhoto.Width, actualPhoto.Width);
            Assert.AreEqual(expectedPhoto.Height, actualPhoto.Height);
            
        }
    }
}
