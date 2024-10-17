using System.ComponentModel.DataAnnotations;
using MoviesAPI.Interfaces;

namespace MoviesAPI.Entities;

public class Movie: IId
{
    public int Id { get; set; }
    [Required]
    [StringLength(300)]
    public string Title { get; set; }
    public bool InCinema { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Poster { get; set; }

    public List<MovieGenre> MoviesGenres { get; set; }
    public List<MovieActor> MoviesActors { get; set; }
}