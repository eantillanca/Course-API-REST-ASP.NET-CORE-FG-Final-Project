using System.ComponentModel.DataAnnotations;
using MoviesAPI.Validations;

namespace MoviesAPI.Dtos;

public class ActorCreateDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    [FileSizeValidation(maxSizeImageMb: 4)]
    [FileTypeValidation(fileTypeGroup: FileTypeGroup.Image)]
    public IFormFile Photo { get; set; }
}