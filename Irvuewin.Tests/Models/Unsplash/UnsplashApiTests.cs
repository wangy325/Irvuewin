using Irvuewin.Models.Unsplash;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using System.Net;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;
using JetBrains.Annotations;

namespace Irvuewin.Tests.Models.Unsplash
{

    // [TestSubject(typeof(UnsplashApi))]
    [TestClass]
    public class UnsplashApiTests
    {
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
           

            var helper = new UnsplashApi(wrapper);

            // Act
            var actualPhoto = await helper.GetPhotoInfoById("0RbbLWm6rLk");

            // Assert
            Assert.IsNotNull(actualPhoto);
            Assert.AreEqual(expectedPhoto.Id, actualPhoto.Id);
            Assert.AreEqual(expectedPhoto.Width, actualPhoto.Width);
            Assert.AreEqual(expectedPhoto.Height, actualPhoto.Height);
        }

        [TestMethod]
        public async Task GetAsync_HttpRequestError_ReturnsDefault()
        {
            // Arrange
            var mockHttpClient = new Mock<IHttpService>();
            mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Simulated network error"));

            var helper = new UnsplashApi(mockHttpClient.Object);

            // Act
            var result = await helper.GetAsync<UnsplashPhoto>("photos/random");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAsync_InvalidJsonResponse_ReturnsDefault()
        {
            // Arrange
            var mockHttpClient = new Mock<IHttpService>();
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"invalid\":\"json\"}")
            };

            mockHttpClient.Setup(client => client.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(mockResponse);

            var helper = new UnsplashApi(mockHttpClient.Object);

            // Act
            var result = await helper.GetAsync<UnsplashPhoto>("photos/random");

            // Assert
            Assert.IsNull(result);
        }
    }
}