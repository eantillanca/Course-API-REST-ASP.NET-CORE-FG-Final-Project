using System.ComponentModel.DataAnnotations;
using MoviesAPI.Interfaces;

namespace MoviesAPI.Entities;

public class CinemaRoom: IId
{
    public int Id { get; set; }
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public List<MovieCinemaRoom> MoviesCinemaRooms { get; set; }
}