using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Dtos;

public class GenreDto
{
    public int Id { get; set; }
    [Required]
    [StringLength(40)]
    public string Name { get; set; }
}