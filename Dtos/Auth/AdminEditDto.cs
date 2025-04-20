using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Dtos.Auth;

public class AdminEditDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
