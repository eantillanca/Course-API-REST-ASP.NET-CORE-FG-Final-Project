
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MoviesAPITest;

public class UserFakeFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Simulate a user with specific claims
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("isAdmin", "1")
        }));

        context.HttpContext.User = user;

        await next();
    }
}