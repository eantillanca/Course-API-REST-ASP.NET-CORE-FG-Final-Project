using AutoMapper;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Dtos;
using MoviesAPI.Helpers;
using MoviesAPI.Interfaces;

namespace MoviesAPI.Controllers;

public class CustomBaseController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    protected CustomBaseController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    protected async Task<List<TDto>> Get<TEntity, TDto>
        (PaginationDto paginationDto) where TEntity : class
    {
        var queryable = _context.Set<TEntity>().AsQueryable();
        return await Get<TEntity, TDto>(paginationDto, queryable);
    }

    protected async Task<List<TDto>> Get<TEntity, TDto>
        (PaginationDto paginationDto, IQueryable<TEntity> queryable) where TEntity : class
    {
        await HttpContext.InsertPaginationParams(queryable, paginationDto.ElementsPerPage);
        var entity = await queryable.Paginate(paginationDto).ToListAsync();

        var dtos = _mapper.Map<List<TDto>>(entity);

        return dtos;
    }

    protected async Task<List<TDto>> Get<TEntity, TDto>() where TEntity : class
    {
        var entities = await _context.Set<TEntity>().AsNoTracking().ToListAsync();
        var dtos = _mapper.Map<List<TDto>>(entities);
        return dtos;
    }

    protected async Task<ActionResult<TDto>> Get<TEntity, TDto>(int id) where TEntity : class, IId
    {
        var entity = await _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            return NotFound();
        }

        var dto = _mapper.Map<TDto>(entity);
        return dto;
    }

    protected async Task<ActionResult> Post<TCreateDto, TEntity, TReadDto>
        (TCreateDto createDto, string routeName) where TEntity : class, IId
    {
        var entity = _mapper.Map<TEntity>(createDto);
        _context.Add(entity);
        await _context.SaveChangesAsync();
        var dto = _mapper.Map<TReadDto>(entity);
        return new CreatedAtRouteResult(routeName, new { id = entity.Id }, dto);
    }

    protected async Task<ActionResult> Put<TCreateDto, TEntity>
        (int id, TCreateDto createDto) where TEntity : class, IId
    {
        var exists = await _context.Set<TEntity>().AnyAsync(x => x.Id == id);
        if (!exists)
        {
            return NotFound();
        }

        var entity = _mapper.Map<TEntity>(createDto);
        entity.Id = id;

        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    protected async Task<ActionResult> Patch<TEntity, TPatchDto>(int id, JsonPatchDocument<TPatchDto> patchDocument)
        where TPatchDto : class
        where TEntity : class, IId
    {
        if (patchDocument == null) { return BadRequest(); }

        var entity = await _context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) { return NotFound(); }

        var dto = _mapper.Map<TPatchDto>(entity);

        patchDocument.ApplyTo(dto, ModelState);

        var isValid = TryValidateModel(dto);

        if (!isValid)
        {
            return BadRequest(ModelState);
        }

        _mapper.Map(dto, entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    protected async Task<ActionResult> Delete<TEntity>
        (int id) where TEntity : class, IId, new()
    {
        var exists = await _context.Set<TEntity>().AnyAsync(x => x.Id == id);
        if (!exists)
        {
            return NotFound();
        }

        _context.Remove(new TEntity() { Id = id });
        await _context.SaveChangesAsync();

        return NoContent();
    }
}