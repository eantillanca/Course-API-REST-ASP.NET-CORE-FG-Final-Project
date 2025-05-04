
namespace MoviesAPI.Dtos;

public class ReviewDto
{
    public int Id { get; set; }
    public string Comment { get; set; }
    public int Calification { get; set; }
    public int MovieId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
}
