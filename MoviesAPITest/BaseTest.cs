using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI;
using MoviesAPI.Helpers;
using NetTopologySuite;

namespace MoviesAPITest;

public class BaseTest
{
    protected ApplicationDbContext BuildContext(string dbName = "TestDatabase")
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    protected IMapper ConfigAutoMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            cfg.AddProfile(new AutoMapperProfile(geometryFactory));
        });

        return config.CreateMapper();
    }

    protected ControllerContext BuildControllerContext()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("isAdmin", "1")
        }));

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }
}
