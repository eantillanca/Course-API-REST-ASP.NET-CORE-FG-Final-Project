using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;

namespace MoviesAPI.Controllers;

[ApiController]
[Route("api/genre")]
public class GenreController: CustomBaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GenreController(ApplicationDbContext context, IMapper mapper)
        :base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<GenreDto>>> Get()
    {
        var genresDtos = await Get<Genre, GenreDto>();
        return Ok(genresDtos);
    }

    [HttpGet("{id:int}", Name = "getGenreById")]
    public async Task<ActionResult<GenreDto>> GetById(int id)
    {
        return await Get<Genre, GenreDto>(id);
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] GenreCreateDto genreCreateDto)
    {
        return await Post<GenreCreateDto, Genre, GenreDto>(genreCreateDto, "getGenreById");
    }

    [HttpPut("{id:int}", Name = "updateGenreById")]
    public async Task<ActionResult> Put(int id, [FromBody] GenreCreateDto genreCreateDto)
    {
        return await Put<GenreCreateDto, Genre>(id, genreCreateDto);
    }

    [HttpDelete("{id:int}", Name = "deleteGenreById")]
    public async Task<ActionResult> Delete(int id)
    {
        return await Delete<Genre>(id);
    }
}
