using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;

namespace MoviesAPI.Controllers;

[ApiController]
[Route("api/cinema-rooms")]
public class CinemaRoomsController: CustomBaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CinemaRoomsController(ApplicationDbContext context, IMapper mapper): base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
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
}
