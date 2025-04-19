using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Dtos;

public class CinemaRoomNearbyFilterDto
{
    [Required]
    public double? Latitude { get; set; }
    [Required]
    public double? Longitude { get; set; }
    private const int maxDistanceKms = 10000; // Maximum distance in kilometers

    private int? distanceKms;

    [Required]
    public int? DistanceKms
    {
        get => distanceKms;
        set => distanceKms = (value.HasValue && value > maxDistanceKms) ? maxDistanceKms : value;
    }
}