using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;
using MoviesAPI.Interfaces;

namespace MoviesAPI.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController: ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFileStorage _fileStorage;
    private readonly string _folder = "movies";

    public MoviesController(ApplicationDbContext context, IMapper mapper, IFileStorage fileStorage)
    {
        _context = context;
        _mapper = mapper;
        _fileStorage = fileStorage;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<MovieDto>>> Get()
    {
        var moviesDb = await _context.Movies.ToListAsync();
        var moviesDtos = _mapper.Map<List<MovieDto>>(moviesDb);
        return Ok(moviesDtos);
    }
    
    [HttpGet("{id:int}", Name = "getMovieById")]
    public async Task<ActionResult<MovieDto>> GetById(int id)
    {
        var movieDb = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
        if (movieDb == null)
        {
            return NotFound();
        }
        var moviesDto = _mapper.Map<MovieDto>(movieDb);
        return Ok(moviesDto);
    }
    
    [HttpPost]
    public async Task<ActionResult> Post([FromForm] MovieCreateDto movieCreateDto)
    {
        var movieDb = _mapper.Map<Movie>(movieCreateDto);

        if (movieCreateDto.Poster != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await movieCreateDto.Poster.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();
                var extension = Path.GetExtension(movieCreateDto.Poster.FileName);

                movieDb.Poster = await _fileStorage.SaveFile(content, extension, _folder, movieCreateDto.Poster.ContentType);
            }
        }
        
        _context.Add(movieDb);
        await _context.SaveChangesAsync();
        var movieDto = _mapper.Map<MovieDto>(movieDb);

        return new CreatedAtRouteResult("getMovieById", new { id = movieDto.Id}, movieDto);
    }
    
    [HttpPut("{id:int}", Name = "updateMovieById")]
    public async Task<ActionResult> Put(int id, [FromForm] MovieCreateDto movieCreateDto)
    {
        var movieDb = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
        
        if (movieDb == null)
        {
            return NotFound();
        }
        
        movieDb = _mapper.Map(movieCreateDto, movieDb);
        
        if (movieCreateDto.Poster != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await movieCreateDto.Poster.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();
                var extension = Path.GetExtension(movieCreateDto.Poster.FileName);

                movieDb.Poster = await _fileStorage.UpdateFile(content, extension, _folder, movieDb.Poster, movieCreateDto.Poster.ContentType);
            }
        }
        
        _context.Entry(movieDb).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    [HttpPatch("{id:int}", Name = "patchMovieById")]
    public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<MoviePatchDto> patchDocument)
    {
        if (patchDocument == null) { return BadRequest(); }

        var movieDb = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
        if (movieDb == null) { return NotFound(); }

        var movieDto = _mapper.Map<MoviePatchDto>(movieDb);
        
        patchDocument.ApplyTo(movieDto, ModelState);

        var isValid = TryValidateModel(movieDto);

        if (!isValid)
        {
            return BadRequest(ModelState);
        }

        _mapper.Map(movieDto, movieDb);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpDelete("{id:int}", Name = "deleteMovieById")]
    public async Task<ActionResult> Delete(int id)
    {
        var movieDb = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
        
        if (movieDb == null)
        {
            return NotFound();
        }

        if (movieDb.Poster != null)
        {
            await _fileStorage.DeleteFile(movieDb.Poster, _folder);
        }

        _context.Remove(movieDb);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}