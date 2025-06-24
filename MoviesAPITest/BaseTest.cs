using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    protected WebApplicationFactory<Startup> BuildWebApplicationFactory(
        string dbName,
        bool ignoreSecurity = true
    )
    {

        var factory = new WebApplicationFactory<Startup>();
        factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing ApplicationDbContext registration
                var descriptorDbContext = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ApplicationDbContext));
                if (descriptorDbContext != null)
                {
                    services.Remove(descriptorDbContext);
                }

                // Add the in-memory database context
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName: dbName);
                });

                if (ignoreSecurity)
                {
                    services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                    services.AddControllers(options =>
                    {
                        options.Filters.Add(new UserFakeFilter());
                    });
                }
            });
        });

        return factory;
    }
}
