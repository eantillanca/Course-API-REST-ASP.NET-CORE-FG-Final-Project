using MoviesAPI.Dtos;

namespace MoviesAPI.Helpers;

public static class QueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginationDto paginationDto)
    {
        return queryable
            .Skip((paginationDto.Page - 1) * paginationDto.ElementsPerPage)
            .Take(paginationDto.ElementsPerPage);
    }
}