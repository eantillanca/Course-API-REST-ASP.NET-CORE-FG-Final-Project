namespace MoviesAPI.Dtos;

public class MoviesFilterDto
{
    public int Page { get; set; }
    public int ElementsPerPage { get; set; } = 10;
    public PaginationDto Pagination { get; set; } = new PaginationDto();
    
    public string Title { get; set; } = "";
    public bool InCinema { get; set; } = false;
    public bool NextPremiers { get; set; } = false;
    public int GenreId { get; set; } = 0;
    public string OrderBy { get; set; }
    public string OrderType { get; set; }
}