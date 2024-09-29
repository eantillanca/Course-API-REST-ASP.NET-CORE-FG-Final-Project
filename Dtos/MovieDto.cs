using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Dtos;

public class MovieDto
{
    public int Id { get; set; }
    [Required]
    [StringLength(300)]
    public string Title { get; set; }
    public bool InCinema { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Poster { get; set; }
}