using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Dtos;

public class GenreCreateDto
{
    [Required]
    [StringLength(40)]
    public string Name { get; set; }
}