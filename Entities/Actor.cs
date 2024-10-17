using System.ComponentModel.DataAnnotations;
using MoviesAPI.Interfaces;

namespace MoviesAPI.Entities;

public class Actor: IId
{
    public int Id { get; set; }
    [Required]
    [StringLength(120)]
    public string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Photo { get; set; }

    public List<MovieActor> MoviesActors { get; set; }
}