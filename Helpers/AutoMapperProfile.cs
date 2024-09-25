using AutoMapper;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;

namespace MoviesAPI.Helpers;

public class AutoMapperProfile: Profile
{
    public AutoMapperProfile()
    {
        // Genre
        CreateMap<Genre, GenreDto>().ReverseMap();
        CreateMap<GenreCreateDto, Genre>();
        
        // Actor
        CreateMap<Actor, ActorDto>().ReverseMap();
        CreateMap<ActorCreateDto, Actor>()
            .ForMember(x => x.Photo, opt => opt.Ignore());
        CreateMap<ActorPatchDto, Actor>().ReverseMap();
    }
}
