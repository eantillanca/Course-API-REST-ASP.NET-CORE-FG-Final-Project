using System.ComponentModel.DataAnnotations;
using MoviesAPI.Interfaces;
using NetTopologySuite.Geometries;

namespace MoviesAPI.Entities;

public class CinemaRoom: IId
{
    public int Id { get; set; }
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    public Point Location { get; set; }

    public List<MovieCinemaRoom> MoviesCinemaRooms { get; set; }
}