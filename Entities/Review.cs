using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MoviesAPI.Interfaces;

namespace MoviesAPI.Entities;

public class Review : IId
{
    public int Id { get; set; }
    public string Comment { get; set; }
    [Range(1, 5)]
    public int Calification { get; set; }
    public int MovieId { get; set; }
    public Movie Movie { get; set; }
    public string UserId { get; set; }
    public IdentityUser User { get; set; }
}
