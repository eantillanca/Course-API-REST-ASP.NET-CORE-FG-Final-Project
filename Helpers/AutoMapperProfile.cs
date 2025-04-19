using AutoMapper;
using MoviesAPI.Dtos;
using MoviesAPI.Entities;
using NetTopologySuite.Geometries;

namespace MoviesAPI.Helpers;

public class AutoMapperProfile: Profile
{
    public AutoMapperProfile(GeometryFactory geometryFactory)
    {
        // Genre
        CreateMap<Genre, GenreDto>().ReverseMap();
        CreateMap<GenreCreateDto, Genre>();
        
        // CinemaRoom
        CreateMap<CinemaRoom, CinemaRoomDto>()
            .ForMember(dto => dto.Latitude, opt => opt.MapFrom(entity => entity.Location.Y))
            .ForMember(dto => dto.Longitude, opt => opt.MapFrom(entity => entity.Location.X));
        CreateMap<CinemaRoomDto, CinemaRoom>()
            .ForMember(entity => entity.Location, opt => 
                opt.MapFrom(dto => geometryFactory.CreatePoint( 
                        new Coordinate(dto.Longitude, dto.Latitude)
                    )
                )
            );
        CreateMap<CinemaRoomCreateDto, CinemaRoom>()
            .ForMember(entity => entity.Location, opt => 
                opt.MapFrom(dto => geometryFactory.CreatePoint( 
                        new Coordinate(dto.Longitude, dto.Latitude)
                    )
                )
            );;
        
        // Actor
        CreateMap<Actor, ActorDto>().ReverseMap();
        CreateMap<ActorCreateDto, Actor>()
            .ForMember(x => x.Photo, opt => opt.Ignore());
        CreateMap<ActorPatchDto, Actor>().ReverseMap();
        
        // Movie
        CreateMap<Movie, MovieDto>()
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.MoviesGenres.Select(mg => mg.Genre)))
            .ForMember(dest => dest.Actors, opt => opt.MapFrom(src => src.MoviesActors.Select(ma => ma.Actor)))
            .ReverseMap();
        CreateMap<MovieCreateDto, Movie>()
            .ForMember(x => x.Poster, opt => opt.Ignore())
            .ForMember(x=> x.MoviesGenres, opt => opt.MapFrom(MapMoviesGenres))
            .ForMember(x=> x.MoviesActors, opt => opt.MapFrom(MapMoviesActors));
        CreateMap<MoviePatchDto, Movie>().ReverseMap();
    }

    private List<MovieGenre> MapMoviesGenres(MovieCreateDto movieCreateDto, Movie movie)
    {
        var result = new List<MovieGenre>();
        if (movieCreateDto.GenreIds == null)
        {
            return result;
        }

        foreach (var id in movieCreateDto.GenreIds)
        {
            result.Add(new MovieGenre() { GenreId = id});
        }
        
        return result;
    }

    private List<MovieActor> MapMoviesActors(MovieCreateDto movieCreateDto, Movie movie)
    {
        var result = new List<MovieActor>();
        if (movieCreateDto.Actors == null)
        {
            return result;
        }

        foreach (var actor in movieCreateDto.Actors)
        {
            result.Add(new MovieActor() { ActorId = actor.ActorId, Character = actor.Character});
        }

        return result;
    }
}
