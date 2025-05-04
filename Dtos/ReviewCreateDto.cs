
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Dtos;

public class ReviewCreateDto
{
    public string Comment { get; set; }
    [Range(1, 5)]
    public int Calification { get; set; }
}
