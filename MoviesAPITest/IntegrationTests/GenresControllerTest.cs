using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;
using Newtonsoft.Json;

namespace MoviesAPITest.IntegrationTests;

[TestClass]
public class GenresControllerTest : BaseTest
{
    private static readonly string url = "api/genre";

    [TestMethod]
    public async Task GetAllGenres_NoGenresInDb()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = BuildWebApplicationFactory(dbName);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var genres = JsonConvert.DeserializeObject<List<GenreDto>>(await response.Content.ReadAsStringAsync());
        Assert.IsNotNull(genres);
        Assert.AreEqual(0, genres.Count);
    }

    [TestMethod]
    public async Task GetAllGenres_OneGenreInDb()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = BuildWebApplicationFactory(dbName);
        var client = factory.CreateClient();

        var context = BuildContext(dbName);
        context.Genres.Add(new Genre { Name = "Action" });
        context.SaveChanges();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
        var genres = JsonConvert.DeserializeObject<List<GenreDto>>(await response.Content.ReadAsStringAsync());
        Assert.IsNotNull(genres);
        Assert.AreEqual(1, genres.Count);
    }

    [TestMethod]
    public async Task GetAllGenres_DeleteGenre()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var factory = BuildWebApplicationFactory(dbName);
        var client = factory.CreateClient();

        var context = BuildContext(dbName);
        var genre = new Genre { Name = "Action" };
        context.Genres.Add(genre);
        context.SaveChanges();

        // Act
        var deleteResponse = await client.DeleteAsync($"{url}/{genre.Id}");

        // Assert
        deleteResponse.EnsureSuccessStatusCode();

        // Verify deletion
        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
