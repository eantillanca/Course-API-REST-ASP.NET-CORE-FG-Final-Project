using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Entities;

namespace MoviesAPI;

public class ApplicationDbContext : IdentityDbContext
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
        modelBuilder.Entity<CinemaRoom>()
            .Property(x => x.Location)
            .HasColumnType("geography");
        modelBuilder.Entity<CinemaRoom>()
            .HasIndex(x => x.Location)
            .HasDatabaseName("IX_CinemaRoom_Location");

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<Actor> Actors { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieGenre> MoviesGenres { get; set; }
    public DbSet<MovieActor> MoviesActors { get; set; }
    public DbSet<CinemaRoom> CinemaRooms { get; set; }
    public DbSet<MovieCinemaRoom> MoviesCinemaRooms { get; set; }
    public DbSet<Review> Reviews { get; set; }
}