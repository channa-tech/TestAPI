using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using TestAPI.Models;
using TestAPI.Services;

public class StoryRepoTests
{
    private readonly StoryRepo _repo;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;

    public StoryRepoTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object);
        _repo = new StoryRepo(_httpClient);
       
    }

    [Fact]
    public async Task GetStories_ShouldReturnListOfStories()
    {
        // Arrange
        await MockObjects();
        // Act
        var stories = await _repo.GetStories();

        // Assert
        Assert.Equal(stories.Count, 3);
        Assert.True(stories.Any(s=>s.Title.Equals("Title1",StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task SearchStories_ShouldReturnMatchingStories()
    {
        // Arrange
       await MockObjects();

        // Act
        var result = await _repo.SearchStories("Title1");

        // Assert
        Assert.Single(result);
        Assert.Equal("Title1", result.First().Title);
    }
   
    [Fact]
    public async Task SearchStories_EmptyName_ShouldReturnAllStories()
    {
        // Arrange
        await MockObjects();
        // Act
        var result = await _repo.SearchStories(string.Empty);

        // Assert
        Assert.Equal(result.Count,3);
        Assert.True(result.Any(s=>s.Title.Equals("Title1",StringComparison.OrdinalIgnoreCase)));
    }
    private async Task MockObjects()
    {
        var storyIds = new[] { "1", "2", "3" };
        var jsonIds = JsonConvert.SerializeObject(storyIds);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonIds)
        };


        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith("newstories.json?print=pretty")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
        var story1 = new { id = 1, title = "Title1", url = "http://example.com", by = "user1" };
        var story2 = new { id = 2, title = "Title2", url = "http://example.com", by = "user2" };
        var story3 = new { id = 3, title = "Title3", url = "http://example.com", by = "user3" };
        var story1Json = JsonConvert.SerializeObject(story1);
        var story2Json = JsonConvert.SerializeObject(story2);
        var story3Json = JsonConvert.SerializeObject(story3);

        var storyResponse1Message = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(story1Json)
        };
        var storyResponse2Message = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(story2Json)
        };
        var storyResponse3Message = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(story3Json)
        };


        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith("item/1.json?print=pretty")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(storyResponse1Message);
        _handlerMock.Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync",
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith("item/2.json?print=pretty")),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(storyResponse2Message);
        _handlerMock.Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync",
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith("item/3.json?print=pretty")),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(storyResponse3Message);
    }
}
