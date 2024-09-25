using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;

namespace MoviesAPI.Controllers;

[ApiController]
[Route("api/genre")]
public class GenreController: ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GenreController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<GenreDto>>> Get()
    {
        var genresDb = await _context.Genres.ToListAsync();
        var genresDtos = _mapper.Map<List<GenreDto>>(genresDb);
        return Ok(genresDtos);
    }

    [HttpGet("{id:int}", Name = "getGenreById")]
    public async Task<ActionResult<GenreDto>> GetById(int id)
    {
        var genreDb = await _context.Genres.FirstOrDefaultAsync(x => x.Id == id);

        if (genreDb == null)
        {
            return NotFound($"Genre with id {id} not found");
        }

        var genreDto = _mapper.Map<GenreDto>(genreDb);
    
        return Ok(genreDto);
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] GenreCreateDto genreCreateDto)
    {
        var genreDb = _mapper.Map<Genre>(genreCreateDto);
        _context.Add(genreDb);
        await _context.SaveChangesAsync();
        var genreDto = _mapper.Map<GenreDto>(genreDb);

        return new CreatedAtRouteResult("getGenreById", new { id = genreDto.Id}, genreDto);
    }

    [HttpPut("{id:int}", Name = "updateGenreById")]
    public async Task<ActionResult> Put(int id, [FromBody] GenreCreateDto genreCreateDto)
    {
        var exists = await _context.Genres.AnyAsync(x => x.Id == id);
        if (!exists)
        {
            return NotFound();
        }
        
        var genreDb = _mapper.Map<Genre>(genreCreateDto);
        genreDb.Id = id;
        
        _context.Entry(genreDb).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete("{id:int}", Name = "deleteGenreById")]
    public async Task<ActionResult> Delete(int id)
    {
        var exists = await _context.Genres.AnyAsync(x => x.Id == id);
        if (!exists)
        {
            return NotFound();
        }

        _context.Remove(new Genre() { Id = id });
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
