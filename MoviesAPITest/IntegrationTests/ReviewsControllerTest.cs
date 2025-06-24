
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;
using Newtonsoft.Json;

namespace MoviesAPITest.IntegrationTests;

[TestClass]
public class ReviewsControllerTest : BaseTest
{
    private static readonly string url = "api/movie/1/review";

    [TestMethod]
    public async Task GetReviews_ReturnNotFound_WhenMovieDoesNotExist()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = BuildWebApplicationFactory(dbName);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetReviews_ReturnEmptyList_WhenNoReviewsExist()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = BuildWebApplicationFactory(dbName);
        var client = factory.CreateClient();
        var context = BuildContext(dbName);
        context.Movies.Add(new Movie { Id = 1, Title = "Test Movie" });
        context.SaveChanges();

        // Act
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var reviews = JsonConvert.DeserializeObject<List<ReviewDto>>(await response.Content.ReadAsStringAsync());
        Assert.AreEqual(0, reviews!.Count);
    }
}