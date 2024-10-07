using System.ComponentModel.DataAnnotations;
using MoviesAPI.Entities;

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
    public List<GenreDto> Genres { get; set; }
    public List<ActorDto> Actors { get; set; }
}