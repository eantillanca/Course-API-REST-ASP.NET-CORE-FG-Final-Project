using Microsoft.EntityFrameworkCore;
using MoviesAPI.Entities;

namespace MoviesAPI;

public class ApplicationDbContext: DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
        //
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MovieActor>()
            .HasKey(x => new { x.MovieId, x.ActorId });
        modelBuilder.Entity<MovieGenre>()
            .HasKey(x => new { x.MovieId, x.GenreId });
        modelBuilder.Entity<MovieCinemaRoom>()
            .HasKey(x => new { x.MovieId, x.CinemaRoomId });
    }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<Actor> Actors { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieGenre> MoviesGenres { get; set; }
    public DbSet<MovieActor> MoviesActors { get; set; }
    public DbSet<CinemaRoom> CinemaRooms { get; set; }
    public DbSet<MovieCinemaRoom> MoviesCinemaRooms { get; set; }
}   