
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Interfaces;
using MoviesAPI.Services;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace MoviesAPI;

public class Startup(IConfiguration configuration)
{
    private IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString)) { connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING"); }
        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseSqlServer(connectionString));
        services.AddAutoMapper(typeof(Startup));
        services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
        services.AddEndpointsApiExplorer();
        services.AddTransient<IFileStorage, LocalStorageService>();
        services.AddHttpContextAccessor();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();

        app.UseRouting();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
