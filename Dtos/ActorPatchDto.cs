using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Dtos;

public class ActorPatchDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
}
