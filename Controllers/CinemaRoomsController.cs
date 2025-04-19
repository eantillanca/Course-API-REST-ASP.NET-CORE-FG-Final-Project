using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;
using NetTopologySuite.Geometries;

namespace MoviesAPI.Controllers;

[ApiController]
[Route("api/cinema-rooms")]
public class CinemaRoomsController : CustomBaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly GeometryFactory _geometryFactory;

    public CinemaRoomsController(ApplicationDbContext context, IMapper mapper, GeometryFactory geometryFactory) : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
        _geometryFactory = geometryFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<CinemaRoomDto>>> Get()
    {
        return await Get<CinemaRoom, CinemaRoomDto>();
    }

    [HttpGet("{id:int}", Name = "getCinemaRoomById")]
    public async Task<ActionResult<CinemaRoomDto>> GetById(int id)
    {
        return await Get<CinemaRoom, CinemaRoomDto>(id);
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] CinemaRoomCreateDto cinemaRoomCreateDto)
    {
        return await Post<CinemaRoomCreateDto, CinemaRoom, CinemaRoomDto>(cinemaRoomCreateDto, "getCinemaRoomById");
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, [FromBody] CinemaRoomCreateDto cinemaRoomCreateDto)
    {
        return await Put<CinemaRoomCreateDto, CinemaRoom>(id, cinemaRoomCreateDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        return await Delete<CinemaRoom>(id);
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<List<CinemaRoomNearbyDto>>> GetNearby([FromQuery] CinemaRoomNearbyFilterDto filter)
    {
        if (!filter.Latitude.HasValue || !filter.Longitude.HasValue || !filter.DistanceKms.HasValue)
        {
            return BadRequest("Latitude, Longitude, and DistanceKms are required.");
        }

        var userLocation = _geometryFactory.CreatePoint(new Coordinate(filter.Longitude.Value, filter.Latitude.Value));

        var nearbyCinemaRooms = await _context.CinemaRooms
            .OrderBy(cr => cr.Location.Distance(userLocation)) // Order by distance
            .Where(cr => cr.Location.Distance(userLocation) <= filter.DistanceKms.Value * 1000)
            .Select(cr => new CinemaRoomNearbyDto
            {
                Id = cr.Id,
                Name = cr.Name,
                Latitude = cr.Location.Y,
                Longitude = cr.Location.X,
                DistanceInKms = Math.Round(cr.Location.Distance(userLocation) / 1000, 2) // Convert meters to kilometers
            })
            .ToListAsync();

        return Ok(nearbyCinemaRooms);
    }

    // NOTE: Example of how to use the STDistance method in SQL Server with Geography data type
    // 
    // DECLARE @MyLocation GEOGRAPHY = 'POINT(-72.6008755 -38.7367466)';
    // SELECT 
    //      Id, Name, 
    //      Location.ToString() as Location, -- POINT format
    //      Location.STDistance(@MyLocation)/1000 as Distance -- in km 
    // FROM CinemaRooms
    // WHERE Location.STDistance(@MyLocation)/1000 < 10 -- < 10 km
    // ORDER BY Location.STDistance(@MyLocation) ASC;
}
