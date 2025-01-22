using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Dtos;

public class CinemaRoomCreateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
}