using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Helpers;
using MoviesAPI.Validations;

namespace MoviesAPI.Dtos;

public class MovieCreateDto
{
    [Required]
    [StringLength(300)]
    public string Title { get; set; }
    public bool InCinema { get; set; }
    public DateTime ReleaseDate { get; set; }
    [FileSizeValidation(maxSizeImageMb: 4)]
    [FileTypeValidation(fileTypeGroup: FileTypeGroup.Image)]
    public IFormFile Poster { get; set; }
    
    [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
    public List<int> GenreIds { get; set; }
    
    [ModelBinder(BinderType = typeof(TypeBinder<List<MoviesActorsCreateDto>>))]
    public List<MoviesActorsCreateDto> Actors { get; set; }
}