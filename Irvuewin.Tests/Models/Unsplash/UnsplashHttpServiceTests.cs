using System.Net;
using System.Text;
using Irvuewin.Helpers;
using Irvuewin.Helpers.HTTP;
using Irvuewin.Models.Unsplash;
using Moq;
using Newtonsoft.Json;

namespace Irvuewin.Tests.Models.Unsplash;

[TestClass]
public class UnsplashHttpServiceTests
{
    private Mock<IHttpClient> _mockHttpClient = null!;
    private UnsplashHttpService _helper = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockHttpClient = new Mock<IHttpClient>();
        // _mockHttpClient.Setup(x => x.Client());
        _helper = new UnsplashHttpService(_mockHttpClient.Object);
    }

    [TestMethod]
    public async Task GetAsyncChannelById_ReturnDeserializedUnsplashChannel()
    {
        var mockResponseContent = JsonConvert.SerializeObject(UnsplashDataSet.ExpectedChannel, JsonHelper.Settings);
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(mockResponseContent, Encoding.UTF8, "application/json")
        };
        var url = $"{UnsplashDataSet.BaseUrl}/collections/{UnsplashDataSet.ChannelId}";
        _mockHttpClient.Setup(x => x.GetAsync(url)).ReturnsAsync(mockResponse);

        var actualChannel = await _helper.GetChannelById(UnsplashDataSet.ChannelId);
        
        Assert.IsNotNull(actualChannel);
        Assert.AreEqual(UnsplashDataSet.ExpectedChannel.Id, actualChannel.Id);
        Assert.AreEqual(UnsplashDataSet.ExpectedChannel.Title, actualChannel.Title);
        Assert.AreEqual(UnsplashDataSet.ExpectedChannel.ShareKey, actualChannel.ShareKey);
        Assert.AreEqual(UnsplashDataSet.ExpectedChannel.UpdatedAt, actualChannel.UpdatedAt);
    }

    [TestMethod]
    public async Task GetAsyncPhotosOfChannel_ReturnsDeserializedUnsplashPhotoList()
    {
        // Arrange
        var query = UnsplashQueryParams.Create().Page(1).PerPage(2);
        var mockResponseContent = JsonConvert.SerializeObject(UnsplashDataSet.ExpectedPhotos, JsonHelper.Settings);
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(mockResponseContent, Encoding.UTF8, "application/json")
        };
        var url = $"{UnsplashDataSet.BaseUrl}/collections/{UnsplashDataSet.ChannelId}/photos?{query.ToQueryString()}";
        _mockHttpClient.Setup(x => x.GetAsync(url)).ReturnsAsync(mockResponse);

        // Act
        var actualPhotos = await _helper.GetPhotosOfChannel(UnsplashDataSet.ChannelId, query);

        // Assert
        Assert.IsNotNull(actualPhotos);
        Assert.AreEqual(UnsplashDataSet.ExpectedPhotos.Count, actualPhotos.Count);
        for (var i = 0; i < UnsplashDataSet.ExpectedPhotos.Count; i++)
        {
            Assert.AreEqual(UnsplashDataSet.ExpectedPhotos[i].Id, actualPhotos[i].Id);
            Assert.AreEqual(UnsplashDataSet.ExpectedPhotos[i].Slug, actualPhotos[i].Slug);
        }
    }

    [TestMethod]
    public async Task GetAsyncPhotoById_ReturnsDeserializedUnsplashPhoto()
    {
        // Arrange
       
        var mockedResponseContent = JsonConvert.SerializeObject(UnsplashDataSet.ExpectedPhoto, JsonHelper.Settings);
        var mockedHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(mockedResponseContent, Encoding.UTF8, "application/json")
        };
        var apiUrl = $"{UnsplashDataSet.BaseUrl}/photos/{UnsplashDataSet.PhotoId}";
        _mockHttpClient.Setup(x => x.GetAsync(apiUrl)).ReturnsAsync(mockedHttpResponse);

        // Act
        var actualPhoto = await _helper.GetPhotoInfoById(UnsplashDataSet.PhotoId);

        // Assert
        Assert.IsNotNull(actualPhoto);
        Assert.AreEqual(UnsplashDataSet.ExpectedPhoto.Id, actualPhoto.Id);
        Assert.AreEqual(UnsplashDataSet.ExpectedPhoto.Width, actualPhoto.Width);
        Assert.AreEqual(UnsplashDataSet.ExpectedPhoto.Height, actualPhoto.Height);
        Assert.AreEqual(UnsplashDataSet.ExpectedPhoto.Slug, actualPhoto.Slug);

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
        // var result = await _helper.GetAsync<UnsplashPhoto>("photos/random");

        // Assert
        // Assert.IsNull(result);
    }
}