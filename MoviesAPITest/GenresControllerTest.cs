using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoviesAPI.Controllers;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;

namespace MoviesAPITest;

[TestClass]
public class GenresControllerTest : BaseTest
{
    [TestMethod]
    public async Task GetGenres_ReturnsAllGenres()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        context.Genres.AddRange(
            new Genre { Name = "Action" },
            new Genre { Name = "Comedy" },
            new Genre { Name = "Drama" }
        );
        context.SaveChanges();

        // Act
        var context2 = BuildContext(dbName);
        var controller = new GenreController(context2, mapper);
        var actionResult = await controller.Get();

        // Assert

        var okResult = actionResult.Result as Microsoft.AspNetCore.Mvc.OkObjectResult;
        Assert.IsNotNull(okResult, "The result is not OkObjectResult");

        var genres = okResult.Value as List<GenreDto>;
        Assert.IsNotNull(genres, "The value is not a List<GenreDto>");

        Assert.AreEqual(3, genres.Count);
        Assert.IsInstanceOfType(genres, typeof(IEnumerable<GenreDto>));
    }

    public async Task GetGenreById_NotFound_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        context.Genres.Add(new Genre { Name = "Action" });
        context.SaveChanges();

        // Act
        var context2 = BuildContext(dbName);
        var controller = new GenreController(context2, mapper);
        var actionResult = await controller.GetById(999); // Non-existing ID

        // Assert
        Assert.IsInstanceOfType(actionResult.Result, typeof(Microsoft.AspNetCore.Mvc.NotFoundResult));
    }

    public async Task GetGenreById_ReturnsGenre()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var genre = new Genre { Name = "Action" };
        context.Genres.Add(genre);
        context.SaveChanges();

        // Act
        var context2 = BuildContext(dbName);
        var controller = new GenreController(context2, mapper);
        var actionResult = await controller.GetById(genre.Id);

        // Assert
        var okResult = actionResult.Result as Microsoft.AspNetCore.Mvc.OkObjectResult;
        Assert.IsNotNull(okResult, "The result is not OkObjectResult");

        var genreDto = okResult.Value as GenreDto;
        Assert.IsNotNull(genreDto, "The value is not a GenreDto");
        Assert.AreEqual(genre.Name, genreDto.Name);
    }

    public async Task PostGenre_ReturnsCreatedAtAction()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var mapper = ConfigAutoMapper();

        var genreCreateDto = new GenreCreateDto { Name = "Action" };

        // Act
        var context = BuildContext(dbName);
        var controller = new GenreController(context, mapper);
        var actionResult = await controller.Post(genreCreateDto);

        // Assert
        var createdAtRouteResult = actionResult as Microsoft.AspNetCore.Mvc.CreatedAtRouteResult;
        Assert.IsNotNull(createdAtRouteResult, "The result is a CreatedAtRouteResult");

        var genreDto = createdAtRouteResult.Value as GenreDto;
        Assert.IsNotNull(genreDto, "The value is not null");
        Assert.AreEqual(genreCreateDto.Name, genreDto.Name);
        Assert.AreEqual("getGenreById", createdAtRouteResult.RouteName);
        Assert.IsNotNull(createdAtRouteResult.RouteValues, "RouteValues should not be null");
        Assert.IsTrue(createdAtRouteResult.RouteValues.ContainsKey("id"), "The route values should contain 'id'");
    }

    public async Task PutGenre_ReturnsNoContent()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var genre = new Genre { Name = "Action" };
        context.Genres.Add(genre);
        context.SaveChanges();

        var genreUpdateDto = new GenreCreateDto { Name = "Updated Action" };

        // Act
        var context2 = BuildContext(dbName);
        var controller = new GenreController(context2, mapper);
        var actionResult = await controller.Put(genre.Id, genreUpdateDto);

        var context3 = BuildContext(dbName);
        var updatedGenre = context3.Genres.FirstOrDefault(x => x.Name == "Updated Action");

        // Assert
        Assert.IsInstanceOfType(actionResult, typeof(Microsoft.AspNetCore.Mvc.NoContentResult));
        Assert.IsNotNull(updatedGenre, "The updated genre should not be null");
        Assert.IsTrue(updatedGenre.Name == "Updated Action", "The genre name should be updated");
    }

    public async Task DeleteGenre_ReturnsNoContent()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var genre = new Genre { Name = "Action" };
        context.Genres.Add(genre);
        context.SaveChanges();

        // Act
        var context2 = BuildContext(dbName);
        var controller = new GenreController(context2, mapper);
        var actionResult = await controller.Delete(genre.Id);

        // Assert
        Assert.IsInstanceOfType(actionResult, typeof(Microsoft.AspNetCore.Mvc.NoContentResult));

        var context3 = BuildContext(dbName);
        var deletedGenre = await context3.Genres.AnyAsync();
        Assert.IsFalse(deletedGenre, "The genre should be deleted");
    }

    public async Task DeleteGenre_NotFound_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        // Act
        var controller = new GenreController(context, mapper);
        var actionResult = await controller.Delete(999); // Non-existing ID

        // Assert
        Assert.IsInstanceOfType(actionResult, typeof(Microsoft.AspNetCore.Mvc.NotFoundResult));
    }
}
