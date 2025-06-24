using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MoviesAPI;
using MoviesAPI.Helpers;
using NetTopologySuite;
using System.Linq;

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
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                    .ToList();
                foreach (var d in descriptors)
                {
                    services.Remove(d);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    ctx.Database.EnsureCreated();
                }

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
