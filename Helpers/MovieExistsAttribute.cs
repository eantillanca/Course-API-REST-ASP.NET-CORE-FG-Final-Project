using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace MoviesAPI.Helpers;

public class MovieExistsAttribute : Attribute, IAsyncResourceFilter
{
    private readonly ApplicationDbContext _context;

    public MovieExistsAttribute(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task OnResourceExecutionAsync(
        ResourceExecutingContext context,
        ResourceExecutionDelegate next
    )
    {
        var movieIdValue = context.RouteData.Values["movieId"]?.ToString();
        if (!int.TryParse(movieIdValue, out var movieId))
        {
            context.Result = new BadRequestResult();
            return;
        }
        Console.WriteLine($"Checking if movie with ID {movieId} exists...");
        var movieExists = await _context.Movies.AnyAsync(x => x.Id == movieId);

        if (!movieExists)
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }
}
