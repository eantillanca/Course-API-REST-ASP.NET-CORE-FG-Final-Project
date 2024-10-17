using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using MoviesAPI.Interfaces;
using System.Linq.Dynamic.Core;

namespace MoviesAPI.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController: CustomBaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<MoviesController> _logger;
    private readonly string _folder = "movies";

    public MoviesController(ApplicationDbContext context, IMapper mapper, IFileStorage fileStorage,
        ILogger<MoviesController> logger): base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
        _fileStorage = fileStorage;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<MoviesIndexDto>> Get()
    {
        var top = 20;
        var today = DateTime.Today;

        var nextPremiers = await _context.Movies
            .Include(x => x.MoviesActors)
                .ThenInclude(ma => ma.Actor) 
            .Include(x => x.MoviesGenres)
                .ThenInclude(mg => mg.Genre) 
            .Where(x => x.ReleaseDate > today)
            .Take(top)
            .ToListAsync();
        var inCinema = await _context.Movies
            .Include(x => x.MoviesActors)
                .ThenInclude(ma => ma.Actor) 
            .Include(x => x.MoviesGenres)
                .ThenInclude(mg => mg.Genre) 
            .Where(x => x.InCinema == true)
            .Take(top)
            .ToListAsync();

        var result = new MoviesIndexDto();
        result.NextPremiers = _mapper.Map<List<MovieDto>>(nextPremiers);
        result.InCinema = _mapper.Map<List<MovieDto>>(inCinema);

        return Ok(result);
    }
    
    [HttpGet("{id:int}", Name = "getMovieById")]
    public async Task<ActionResult<MovieDto>> GetById(int id)
    {
        var movieDb = await _context.Movies
            .Include(x => x.MoviesActors)
                .ThenInclude(ma => ma.Actor) 
            .Include(x => x.MoviesGenres)
                .ThenInclude(mg => mg.Genre) 
            .FirstOrDefaultAsync(x => x.Id == id);
        if (movieDb == null)
        {
            return NotFound();
        }
        var moviesDto = _mapper.Map<MovieDto>(movieDb);
        return Ok(moviesDto);
    }

    [HttpGet("filter")]
    public async Task<ActionResult<List<MovieDto>>> Filter([FromQuery] MoviesFilterDto moviesFilterDto)
    {
        var moviesQueriable = _context.Movies
            .Include(x => x.MoviesGenres).ThenInclude(mg => mg.Genre)
            .Include(x => x.MoviesActors).ThenInclude(ma => ma.Actor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(moviesFilterDto.Title))
        {
            moviesQueriable = moviesQueriable.Where(x => x.Title.Contains(moviesFilterDto.Title));
        }

        if (moviesFilterDto.InCinema)
        {
            moviesQueriable = moviesQueriable.Where(x => x.InCinema);
        }

        if (moviesFilterDto.NextPremiers)
        {
            moviesQueriable = moviesQueriable.Where(x => x.ReleaseDate > DateTime.Today);
        }

        if (moviesFilterDto.GenreId != 0)
        {
            moviesQueriable = moviesQueriable
                .Where(x => x.MoviesGenres.Select(y => y.GenreId)
                    .Contains(moviesFilterDto.GenreId));
        }

        if (!string.IsNullOrEmpty(moviesFilterDto.OrderBy))
        {
            try
            {
                var orderType = moviesFilterDto.OrderType switch
                {
                    "asc" => "ascending",
                    "desc" => "descending",
                    _ => "ascending"
                };
                
                moviesQueriable = moviesQueriable.OrderBy($"{moviesFilterDto.OrderBy} {orderType}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Order by {moviesFilterDto.OrderBy} field not found. Ex: {e.Message}");
            }
        }
        
        if (moviesFilterDto.ElementsPerPage > 0)
        {
            await HttpContext.InsertPaginationParams(moviesQueriable, moviesFilterDto.ElementsPerPage);
            moviesQueriable = moviesQueriable.Paginate(moviesFilterDto.Pagination);
        }

        var movies = await moviesQueriable.ToListAsync();
        
        return _mapper.Map<List<MovieDto>>(movies);
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
        
        AsignActorOrder(movieDb);
        
        _context.Add(movieDb);
        await _context.SaveChangesAsync();
        
        var movieWithRelations = await _context.Movies
            .Include(x => x.MoviesGenres).ThenInclude(mg => mg.Genre)
            .Include(x => x.MoviesActors).ThenInclude(ma => ma.Actor)
            .FirstOrDefaultAsync(x => x.Id == movieDb.Id);

        var movieDto = _mapper.Map<MovieDto>(movieWithRelations);
        
        return new CreatedAtRouteResult("getMovieById", new { id = movieDto.Id}, movieDto);
    }
    
    [HttpPut("{id:int}", Name = "updateMovieById")]
    public async Task<ActionResult> Put(int id, [FromForm] MovieCreateDto movieCreateDto)
    {
        var movieDb = await _context.Movies
            .Include(x => x.MoviesActors)
            .Include(x => x.MoviesGenres)
            .FirstOrDefaultAsync(x => x.Id == id);
        
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
        
        AsignActorOrder(movieDb);
        
        _context.Entry(movieDb).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    [HttpPatch("{id:int}", Name = "patchMovieById")]
    public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<MoviePatchDto> patchDocument)
    {
        return await Patch<Movie, MoviePatchDto>(id, patchDocument);
    }
    
    [HttpDelete("{id:int}", Name = "deleteMovieById")]
    public async Task<ActionResult> Delete(int id)
    {
        return await Delete<Movie>(id);
    }

    private void AsignActorOrder(Movie movie)
    {
        if (movie.MoviesActors != null)
        {
            for (int i = 0; i < movie.MoviesActors.Count; i++)
            {
                movie.MoviesActors[i].Order = i;
            }
        }
    }
}