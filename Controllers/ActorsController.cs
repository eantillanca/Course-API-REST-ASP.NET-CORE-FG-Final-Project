using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;
using MoviesAPI.Interfaces;

namespace MoviesAPI.Controllers;

[ApiController]
[Route("api/actors")]
public class ActorsController: CustomBaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFileStorage _fileStorage;
    private readonly string _folder = "actors";

    public ActorsController(ApplicationDbContext context, IMapper mapper, IFileStorage fileStorage)
        :base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public async Task<ActionResult<List<ActorDto>>> Get([FromQuery] PaginationDto paginationDto)
    {
        var actors = await Get<Actor, ActorDto>(paginationDto);
        return Ok(actors);
    }
    
    [HttpGet("{id:int}", Name = "getActorById")]
    public async Task<ActionResult<ActorDto>> GetById(int id)
    {
        var actorDto = await Get<Actor, ActorDto>(id);
        return Ok(actorDto);
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromForm] ActorCreateDto actorCreateDto)
    {
        var actorDb = _mapper.Map<Actor>(actorCreateDto);

        if (actorCreateDto.Photo != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await actorCreateDto.Photo.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();
                var extension = Path.GetExtension(actorCreateDto.Photo.FileName);

                actorDb.Photo = await _fileStorage.SaveFile(content, extension, _folder, actorCreateDto.Photo.ContentType);
            }
        }
        
        _context.Add(actorDb);
        await _context.SaveChangesAsync();
        var actorDto = _mapper.Map<ActorDto>(actorDb);

        return new CreatedAtRouteResult("getActorById", new { id = actorDto.Id}, actorDto);
    }

    [HttpPut("{id:int}", Name = "updateActorById")]
    public async Task<ActionResult> Put(int id, [FromForm] ActorCreateDto actorCreateDto)
    {
        var actorDb = await _context.Actors.FirstOrDefaultAsync(x => x.Id == id);
        
        if (actorDb == null)
        {
            return NotFound();
        }
        
        actorDb = _mapper.Map(actorCreateDto, actorDb);
        
        if (actorCreateDto.Photo != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await actorCreateDto.Photo.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();
                var extension = Path.GetExtension(actorCreateDto.Photo.FileName);

                actorDb.Photo = await _fileStorage.UpdateFile(content, extension, _folder, actorDb.Photo, actorCreateDto.Photo.ContentType);
            }
        }
        
        _context.Entry(actorDb).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpPatch("{id:int}", Name = "patchActorById")]
    public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<ActorPatchDto> patchDocument)
    {
        return await Patch<Actor, ActorPatchDto>(id, patchDocument);
    }
    
    [HttpDelete("{id:int}", Name = "deleteActorById")]
    public async Task<ActionResult> Delete(int id)
    {
        var actorDb = await _context.Actors.FirstOrDefaultAsync(x => x.Id == id);
        
        if (actorDb == null)
        {
            return NotFound();
        }

        if (actorDb.Photo != null)
        {
            await _fileStorage.DeleteFile(actorDb.Photo, _folder);
        }

        _context.Remove(actorDb);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}