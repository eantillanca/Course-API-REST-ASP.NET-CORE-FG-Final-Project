using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoviesAPI.Entities;
using MoviesAPI.Controllers;
using MoviesAPI.Dtos;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Moq;
using MoviesAPI.Interfaces;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;

namespace MoviesAPITest;

[TestClass]
public class ActorsControllerTest : BaseTest
{
    // Run this test class with the following command:
    // dotnet test --settings test.runsettings --logger "console;verbosity=detailed"  
    // show all Console.WriteLine() output in the test results console

    [TestMethod]
    public async Task GetActors_ReturnsPaginatedActors()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        context.Actors.AddRange(
            new Actor { Name = "Actor 1" },
            new Actor { Name = "Actor 2" },
            new Actor { Name = "Actor 3" }
        );
        context.SaveChanges();

        // Act
        var context2 = BuildContext(dbName);
        var controller = new ActorsController(context2, mapper, null);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        var actionResult = await controller.Get(new PaginationDto
        {
            Page = 1,
            ElementsPerPage = 2
        });
        var actorsPage1 = actionResult.Result as Microsoft.AspNetCore.Mvc.OkObjectResult;

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        var actionResult2 = await controller.Get(new PaginationDto
        {
            Page = 2,
            ElementsPerPage = 2
        });
        var actorsPage2 = actionResult2.Result as OkObjectResult;

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        var actionResult3 = await controller.Get(new PaginationDto
        {
            Page = 3,
            ElementsPerPage = 2
        });
        var actorsPage3 = actionResult3.Result as OkObjectResult;

        // Assert
        Assert.IsNotNull(actorsPage1, "Actors page 1 should not be null");
        Assert.IsNotNull(actorsPage1.Value as List<ActorDto>, "Actors page 1 should contain a list of ActorDto");
        Assert.AreEqual(2, (actorsPage1.Value as List<ActorDto>)!.Count, "Page 1 should return 2 actors");
        Console.WriteLine(JsonSerializer.Serialize(actorsPage1.Value as List<ActorDto>));

        Assert.IsNotNull(actorsPage2, "Actors page 2 should not be null");
        Assert.IsNotNull(actorsPage2.Value as List<ActorDto>, "Actors page 2 should contain a list of ActorDto");
        Assert.AreEqual(1, (actorsPage2.Value as List<ActorDto>)!.Count, "Page 2 should return 1 actor");
        Console.WriteLine(JsonSerializer.Serialize(actorsPage2.Value as List<ActorDto>));

        Assert.IsNotNull(actorsPage3, "Actors page 3 should not be null");
        Assert.IsNotNull(actorsPage3.Value as List<ActorDto>, "Actors page 3 should contain a list of ActorDto");
        Assert.AreEqual(0, (actorsPage3.Value as List<ActorDto>)!.Count, "Page 3 should return 0 actors");
        Console.WriteLine(JsonSerializer.Serialize(actorsPage3.Value as List<ActorDto>));
    }

    public async Task CreateActorWithoutPhoto_ReturnsCreatedActor()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var actorCreateDto = new ActorCreateDto
        {
            Name = "New Actor",
            DateOfBirth = new DateTime(1990, 1, 1),
            Photo = null // No photo provided
        };

        var mock = new Mock<IFileStorage>();
        mock.Setup(x => x.SaveFile(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("path/to/photo.jpg");

        var controller = new ActorsController(context, mapper, mock.Object);
        controller.ControllerContext.HttpContext = new DefaultHttpContext();

        // Act
        var actionResult = await controller.Post(actorCreateDto);
        var createdResult = actionResult as Microsoft.AspNetCore.Mvc.CreatedAtRouteResult;

        // Assert
        Assert.IsNotNull(createdResult, "Created result should not be null");
        Assert.IsNotNull(createdResult.Value as ActorDto, "Created actor should not be null");
        var createdActor = createdResult.Value as ActorDto;
        Assert.AreEqual("New Actor", createdActor!.Name, "Actor name should match the input");
        Assert.IsNull(createdActor.Photo, "Actor photo is null");
        Assert.AreEqual(0, mock.Invocations.Count);
    }

    public async Task CreateActorWithPhoto_ReturnsCreatedActor()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var content = Encoding.UTF8.GetBytes("Test image");
        var file = new FormFile(new MemoryStream(content), 0, content.Length, "Data", "image.jpg");
        file.Headers = new HeaderDictionary();
        file.ContentType = "image/jpg";

        var actorCreateDto = new ActorCreateDto
        {
            Name = "New Actor",
            DateOfBirth = new DateTime(1990, 1, 1),
            Photo = file
        };

        var mock = new Mock<IFileStorage>();
        mock.Setup(x => x.SaveFile(content, ".jpg", "actors", file.ContentType))
            .Returns(Task.FromResult("url"));

        var controller = new ActorsController(context, mapper, mock.Object);
        controller.ControllerContext.HttpContext = new DefaultHttpContext();

        // Act
        var actionResult = await controller.Post(actorCreateDto);
        var createdResult = actionResult as Microsoft.AspNetCore.Mvc.CreatedAtRouteResult;

        // Assert
        Assert.IsNotNull(createdResult, "Created result should not be null");
        Assert.IsNotNull(createdResult.Value as ActorDto, "Created actor should not be null");
        var createdActor = createdResult.Value as ActorDto;
        Assert.AreEqual("New Actor", createdActor!.Name, "Actor name should match the input");
        Assert.AreEqual("url", createdActor.Photo);
        Assert.AreEqual(1, mock.Invocations.Count);
    }

    [TestMethod]
    public async Task PatchActor_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var controller = new ActorsController(context, mapper, null);
        var patchDoc = new JsonPatchDocument<ActorPatchDto>();



        // Act
        var actionResult = await controller.Patch(1, patchDoc);
        var notFoundResult = actionResult as Microsoft.AspNetCore.Mvc.NotFoundResult;

        // Assert
        Assert.IsNotNull(notFoundResult, "Not found result should not be null");
        Assert.AreEqual(404, notFoundResult.StatusCode);
    }

    [TestMethod]
    public async Task PatchActorUpdateActor_ReturnsNoContent()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var context = BuildContext(dbName);
        var mapper = ConfigAutoMapper();

        var dateOfBirth = DateTime.Now;
        var actor = new Actor()
        {
            Name = "Actor 1",
            DateOfBirth = dateOfBirth
        };
        context.Add(actor);
        await context.SaveChangesAsync();

        var context2 = BuildContext(dbName);
        var controller = new ActorsController(context2, mapper, null);
        var objectValidator = new Mock<IObjectModelValidator>();
        objectValidator.Setup(x => x.Validate(
            It.IsAny<ActionContext>(),
            It.IsAny<ValidationStateDictionary>(),
            It.IsAny<string>(),
            It.IsAny<object>()
        ));
        controller.ObjectValidator = objectValidator.Object;

        var patchDoc = new JsonPatchDocument<ActorPatchDto>();
        patchDoc.Operations.Add(new Operation<ActorPatchDto>("replace", "/name", null, "Erik"));

        var context3 = BuildContext(dbName);

        // Act
        var actionResult = await controller.Patch(1, patchDoc);
        var result = actionResult as NoContentResult;

        var actorUpdate = await context3.Actors.FirstAsync();

        // Assert
        Assert.IsNotNull(result, "Not found result should not be null");
        Assert.AreEqual(204, result.StatusCode);
        Assert.AreEqual("Erik", actorUpdate.Name);
        Assert.AreEqual(dateOfBirth, actorUpdate.DateOfBirth);
    }
}