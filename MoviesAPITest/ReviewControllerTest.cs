using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MoviesAPI.Controllers;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;

namespace MoviesAPITest;

[TestClass]
public class ReviewControllerTest : BaseTest
{
    [TestMethod]
    public async Task CreateReview_NoShouldCreateReview()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        CreateMovieData(dbName);

        var movieId = context.Movies.First().Id;
        var review = new Review
        {
            Calification = 5,
            Comment = "Great movie!",
            MovieId = movieId,
            UserId = "test-user-id"
        };
        context.Reviews.Add(review);
        context.SaveChanges();

        var context2 = BuildContext(dbName);
        var loggerMock = new Mock<ILogger<ReviewController>>();
        var mapper = ConfigAutoMapper();
        var controller = new ReviewController(context2, mapper, loggerMock.Object);
        controller.ControllerContext = BuildControllerContext();

        var reviewCreateDto = new ReviewCreateDto
        {
            Calification = 5,
            Comment = "Great movie!"
        };

        // Act
        var result = await controller.Post(movieId, reviewCreateDto);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task CreateReview_ShouldCreateReview()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        CreateMovieData(dbName);
        var movieId = context.Movies.First().Id;
        var context2 = BuildContext(dbName);
        var loggerMock = new Mock<ILogger<ReviewController>>();
        var mapper = ConfigAutoMapper();
        var controller = new ReviewController(context2, mapper, loggerMock.Object);
        controller.ControllerContext = BuildControllerContext();
        var reviewCreateDto = new ReviewCreateDto
        {
            Calification = 5,
            Comment = "Great movie!"
        };
        // Act
        var result = await controller.Post(movieId, reviewCreateDto);

        // Assert
        Assert.IsInstanceOfType(result, typeof(CreatedAtRouteResult));
        var createdAtRouteResult = result as CreatedAtRouteResult;
        Assert.IsNotNull(createdAtRouteResult);
        Assert.AreEqual("Get", createdAtRouteResult.RouteName);
        Assert.IsNotNull(createdAtRouteResult.RouteValues);
        Assert.IsTrue(createdAtRouteResult.RouteValues.ContainsKey("movieId"));
        Assert.AreEqual(movieId, createdAtRouteResult.RouteValues["movieId"]);
        Assert.IsNotNull(createdAtRouteResult.Value);
    }

    private void CreateMovieData(string dbName)
    {
        var context = BuildContext(dbName);
        var movie = new Movie
        {
            Title = "Test Movie"
        };

        context.Movies.Add(movie);
        context.SaveChanges();
    }
}