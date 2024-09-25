using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace MoviesAPI.Helpers;

public static class HttpContextExtensions
{
    public static async Task InsertPaginationParams<T>(this HttpContext httpContext,
        IQueryable<T> queryable, int elementsPerPage)
    {
        double count = await queryable.CountAsync();
        double totalPages = Math.Ceiling(count / elementsPerPage);
        httpContext.Response.Headers["totalPages"] = totalPages.ToString(CultureInfo.InvariantCulture);
    }
}