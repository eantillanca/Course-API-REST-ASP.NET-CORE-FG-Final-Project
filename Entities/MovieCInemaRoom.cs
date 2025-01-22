namespace MoviesAPI.Entities;

public class MovieCinemaRoom
{
    public int MovieId { get; set; }
    public int CinemaRoomId { get; set; }
    public Movie Movie { get; set; }
    public CinemaRoom CinemaRoom { get; set; }
}