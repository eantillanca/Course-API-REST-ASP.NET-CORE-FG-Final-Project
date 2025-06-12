using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoviesAPI.Controllers;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;
using System.Linq.Dynamic.Core;
using Moq;
using Microsoft.Extensions.Logging;

namespace MoviesAPITest;

[TestClass]
public class MoviesControllerTest : BaseTest
{
    private async Task<string> CreateDataTest()
    {
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);

        var genre = new Genre() { Name = "Genre 1" };

        var movies = new List<Movie>()
        {
            new Movie() { Title = "Pelicula 1", ReleaseDate = new DateTime(2024, 1, 1), InCinema = false },
            new Movie() { Title = "No estrenada", ReleaseDate = DateTime.Today.AddDays(1), InCinema = false },
            new Movie() { Title = "Pelicula en cines", ReleaseDate = DateTime.Today.AddDays(-1), InCinema = true }
        };

        var movieWithGenre = new Movie() { Title = "Pelicula con genero", ReleaseDate = new DateTime(2024, 1, 1), InCinema = false };
        context.Add(movieWithGenre);

        context.Add(genre);
        context.Movies.AddRange(movies);
        await context.SaveChangesAsync();

        var movieGenre = new MovieGenre() { MovieId = movieWithGenre.Id, GenreId = genre.Id };
        context.Add(movieGenre);
        await context.SaveChangesAsync();

        return dbName;
    }

    [TestMethod]
    public async Task MoviesFilterByTitle_ReturnsMoviesFilter()
    {
        // Arrange
        var dbName = await CreateDataTest();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var controller = new MoviesController(context, mapper, null, null);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var movieTitle = "Pelicula 1";

        var filterDto = new MoviesFilterDto()
        {
            Title = movieTitle,
            ElementsPerPage = 3
        };

        // Act
        var actionResult = await controller.Filter(filterDto);
        var movies = actionResult.Value as List<MovieDto>;

        // Assert
        Assert.IsNotNull(movies);
        Assert.AreEqual(1, movies.Count);
        Assert.AreEqual(movieTitle, movies[0].Title);
    }

    [TestMethod]
    public async Task MoviesFilterByInCinema_ReturnsMoviesFilter()
    {
        // Arrange
        var dbName = await CreateDataTest();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var controller = new MoviesController(context, mapper, null, null);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var movieTitle = "Pelicula en cines";

        var filterDto = new MoviesFilterDto()
        {
            InCinema = true
        };

        // Act
        var actionResult = await controller.Filter(filterDto);
        var movies = actionResult.Value as List<MovieDto>;

        // Assert
        Assert.IsNotNull(movies);
        Assert.AreEqual(1, movies.Count);
        Assert.AreEqual(movieTitle, movies[0].Title);
    }

    [TestMethod]
    public async Task MoviesFilterByNextPremiers_ReturnsMoviesFilter()
    {
        // Arrange
        var dbName = await CreateDataTest();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var controller = new MoviesController(context, mapper, null, null);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var movieTitle = "No estrenada";

        var filterDto = new MoviesFilterDto()
        {
            NextPremiers = true
        };

        // Act
        var actionResult = await controller.Filter(filterDto);
        var movies = actionResult.Value as List<MovieDto>;

        // Assert
        Assert.IsNotNull(movies);
        Assert.AreEqual(1, movies.Count);
        Assert.AreEqual(movieTitle, movies[0].Title);
    }

    [TestMethod]
    public async Task MoviesFilterByGenre_ReturnsMoviesFilter()
    {
        // Arrange
        var dbName = await CreateDataTest();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var controller = new MoviesController(context, mapper, null, null);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var movieTitle = "Pelicula con genero";
        var genreId = context.Genres.Select(x => x.Id).First();

        var filterDto = new MoviesFilterDto()
        {
            GenreId = genreId
        };

        // Act
        var actionResult = await controller.Filter(filterDto);
        var movies = actionResult.Value as List<MovieDto>;

        // Assert
        Assert.IsNotNull(movies);
        Assert.AreEqual(1, movies.Count);
        Assert.AreEqual(movieTitle, movies[0].Title);
    }

    [TestMethod]
    public async Task MoviesOrderByTitle_ReturnsMoviesOrder()
    {
        // Arrange
        var dbName = await CreateDataTest();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var controller = new MoviesController(context, mapper, null, null);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var filterDto = new MoviesFilterDto()
        {
            OrderBy = "Title",
            OrderType = "asc"
        };

        // Act
        var actionResult = await controller.Filter(filterDto);
        var movies = actionResult.Value as List<MovieDto>;

        var context2 = BuildContext(dbName);
        var moviesDb = context2.Movies.OrderBy(x => x.Title).ToList();

        // Assert
        Assert.IsNotNull(movies);
        Assert.AreEqual(movies.Count, moviesDb.Count);

        for (int i = 0; i < movies.Count; i++)
        {
            var m1 = movies[i];
            var m2 = moviesDb[i];
            Assert.AreEqual(m1.Id, m2.Id);
        }
    }

    [TestMethod]
    public async Task MoviesOrderByTitleDesc_ReturnsMoviesOrder()
    {
        // Arrange
        var dbName = await CreateDataTest();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var controller = new MoviesController(context, mapper, null, null);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var filterDto = new MoviesFilterDto()
        {
            OrderBy = "Title",
            OrderType = "desc"
        };

        // Act
        var actionResult = await controller.Filter(filterDto);
        var movies = actionResult.Value as List<MovieDto>;

        var context2 = BuildContext(dbName);
        var moviesDb = context2.Movies.OrderByDescending(x => x.Title).ToList();

        // Assert
        Assert.IsNotNull(movies);
        Assert.AreEqual(movies.Count, moviesDb.Count);

        for (int i = 0; i < movies.Count; i++)
        {
            var m1 = movies[i];
            var m2 = moviesDb[i];
            Assert.AreEqual(m1.Id, m2.Id);
        }
    }

    [TestMethod]
    public async Task MoviesFilterNotExists_ReturnsMoviesFilter()
    {
        // Arrange
        var dbName = await CreateDataTest();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();
        var mock = new Mock<ILogger<MoviesController>>();

        var controller = new MoviesController(context, mapper, null, mock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };


        var filterDto = new MoviesFilterDto()
        {
            OrderBy = "abc",
            OrderType = "desc"
        };



        // Act
        var actionResult = await controller.Filter(filterDto);
        var movies = actionResult.Value as List<MovieDto>;

        var context2 = BuildContext(dbName);
        var moviesDb = context2.Movies.ToList();

        // Assert
        Assert.IsNotNull(movies);
        Assert.AreEqual(moviesDb.Count, movies.Count);
        Assert.AreEqual(1, mock.Invocations.Count);
    }
}
