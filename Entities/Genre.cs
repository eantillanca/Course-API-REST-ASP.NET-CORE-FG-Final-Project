using System.ComponentModel.DataAnnotations;
using MoviesAPI.Interfaces;

namespace MoviesAPI.Entities;

public class Genre: IId
{
    public int Id { get; set; }
    [Required]
    [StringLength(40)]
    public string Name { get; set; }

    public List<MovieGenre> MoviesGenres { get; set; }
}