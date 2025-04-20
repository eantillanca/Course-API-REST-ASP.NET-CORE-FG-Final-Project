namespace MoviesAPI.Dtos.Auth;

public class AuthenticationResponseDto
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
}
