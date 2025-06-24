using MoviesAPI;
using MoviesAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// start services configuration area

builder.Configuration.AddEnvironmentVariables();

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

// end services configuration area

var app = builder.Build();

// start middleware configuration area

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.InitializeAsync(services).GetAwaiter().GetResult();
}

var serviceLogger = (ILogger<Startup>)app.Services.GetService(typeof(ILogger<Startup>));

startup.Configure(app, app.Environment, serviceLogger);

// end middleware configuration area

app.Run();
