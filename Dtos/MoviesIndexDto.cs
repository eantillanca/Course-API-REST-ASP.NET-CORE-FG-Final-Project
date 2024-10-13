namespace MoviesAPI.Dtos;

public class MoviesIndexDto
{
    public List<MovieDto> NextPremiers { get; set; }
    public List<MovieDto> InCinema { get; set; }
}