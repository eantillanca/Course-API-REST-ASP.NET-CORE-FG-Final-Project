using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;

namespace MoviesAPI.Controllers;

[ApiController]
[ServiceFilter(typeof(MovieExistsAttribute))]
[Route("api/movie/{movieId:int}/review")]
public class ReviewController : CustomBaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<ReviewController> logger
    ) : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet(Name = nameof(Get))]
    public async Task<ActionResult<List<ReviewDto>>> Get(
        int movieId,
        [FromQuery] PaginationDto paginationDto
    )
    {
        var queryable = _context.Reviews
            .Include(x => x.User).AsQueryable();
        queryable = queryable.Where(x => x.MovieId == movieId);
        return await Get<Review, ReviewDto>(paginationDto, queryable);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Post(
        int movieId,
        [FromBody] ReviewCreateDto reviewCreateDto
    )
    {

        var userId = HttpContext.User.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var reviewExist = await _context.Reviews
            .AnyAsync(x => x.MovieId == movieId && x.UserId == userId);
        if (reviewExist)
        {
            return BadRequest("You already have a review for this movie");
        }

        var review = _mapper.Map<Review>(reviewCreateDto);
        review.MovieId = movieId;
        review.UserId = userId;
        _context.Add(review);
        await _context.SaveChangesAsync();

        var reviewDto = _mapper.Map<ReviewDto>(review);
        return new CreatedAtRouteResult(
            nameof(Get),
            new { movieId = movieId },
            reviewDto
        );
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult> Put(
        int id,
        int movieId,
        [FromBody] ReviewCreateDto reviewCreateDto
    )
    {
        var userId = HttpContext.User.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var review = await _context.Reviews
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.UserId != userId)
        {
            return Forbid();
        }

        review = _mapper.Map(reviewCreateDto, review);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> Delete(
        int id,
        int movieId
    )
    {
        var userId = HttpContext.User.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var review = await _context.Reviews
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.UserId != userId)
        {
            return Forbid();
        }

        _context.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
