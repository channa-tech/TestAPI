using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using Service;
using TestAPI.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using TestAPI.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

public class StoryRepoTests
{
    private  IRepository _repo;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;

    public StoryRepoTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object);
        
       
    }
    /// <summary>
    /// Stub to Test end poit '/Stories'
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestGetStoriesController_GetAll()
    {
        await MockObjects();
        Mock<ILogger<StoriesController>> loggerMock = new Mock<ILogger<StoriesController>>();
        var cntroller = new StoriesController(loggerMock.Object,_repo);
        var stories =  cntroller.Get();

        Assert.Equal( 3, stories.Count());
    }
    
    /// <summary>
    /// Stub to Test Search with empty search parameter Endpoint /Stories/Search
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestGetStoriesController_Search_Empty()
    {
        await MockObjects();
        Mock<ILogger<StoriesController>> loggerMock = new Mock<ILogger<StoriesController>>();
        var cntroller = new TestAPI.Controllers.StoriesController(loggerMock.Object, _repo);
        var stories = cntroller.Search("");

        Assert.Equal(3,stories.Count());
    }

    /// <summary>
    /// Stub to Test Search with title Endpoint /Stories/Search?search="Title1"
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    [Theory]
    [InlineData("Title1")]
    public async Task TestGetStoriesController_Search(string search)
    {
        await MockObjects();
        Mock<ILogger<StoriesController>> loggerMock = new Mock<ILogger<StoriesController>>();
        var cntroller = new TestAPI.Controllers.StoriesController(loggerMock.Object, _repo);
        var stories = cntroller.Search(search);

        Assert.True(stories.All(s=>s.Title.Contains(search,StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Stub to Test Cache functionality
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestCacheFunctionality()
    {
        await MockObjects();
        //Calling methods twice.
        await _repo.GetStories();
        await _repo.GetStories();
        // verifying http request is only sent once, next call it just got it from cache.
        _handlerMock.Protected()
            .Verify<Task<HttpResponseMessage>>("SendAsync",Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith("newstories.json?print=pretty")),
                ItExpr.IsAny<CancellationToken>());
    }
    /// <summary>
    /// Stub to test Service layer which fetches all stories
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetStories_ShouldReturnListOfStories()
    {
        // Arrange
        await MockObjects();
        // Act
        var stories = await _repo.GetStories();

        // Assert
        Assert.Equal( 3, stories.Count);
        Assert.True(stories.Any(s=>s.Title.Equals("Title1",StringComparison.OrdinalIgnoreCase)));

    }
    /// <summary>
    /// Stub to test service layer which fetches matching stories
    /// </summary>
    /// <returns></returns>
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
   /// <summary>
   /// Stub to test service layer with empty search parameter
   /// </summary>
   /// <returns></returns>
    [Fact]
    public async Task SearchStories_EmptyName_ShouldReturnAllStories()
    {
        // Arrange
        await MockObjects();
        // Act
        var result = await _repo.SearchStories(string.Empty);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result.Any(s=>s.Title.Equals("Title1",StringComparison.OrdinalIgnoreCase)));
    }
    /// <summary>
    /// Stub to test Global exception filter
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [Fact]
    public async Task TestExceptionMiddleware()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
            })
            .Configure(app =>
            {
                app.UseMiddleware<GlobalExceptionMiddleware>();
                app.Run(async context =>
                {
                    // Simulate an exception
                    throw new InvalidOperationException("Test exception");
                });
            });

        var server = new TestServer(builder);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/");
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("An unexpected error occurred", responseContent);
    }
    /// <summary>
    /// Helper method to mock object
    /// </summary>
    /// <returns></returns>
    private async Task MockObjects()
    {
        var storyIds = new[] { "1", "2", "3" };
        var jsonIds = JsonConvert.SerializeObject(storyIds);
        ICache cache = new Cache();
        _repo = new StoryRepo(_httpClient,cache);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonIds)
        };


        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith("newstories.json?print=pretty")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage).Verifiable(Times.Once);
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
