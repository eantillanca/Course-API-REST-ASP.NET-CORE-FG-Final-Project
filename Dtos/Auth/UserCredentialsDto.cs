using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Dtos.Auth;

public class UserCredentialsDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}
