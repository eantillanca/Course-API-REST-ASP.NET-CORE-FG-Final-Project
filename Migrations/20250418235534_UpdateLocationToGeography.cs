using Microsoft.EntityFrameworkCore.Migrations;

namespace MoviesAPI.Migrations
{
    public partial class UpdateLocationToGeography : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE CinemaRooms
                ALTER COLUMN Location geography
            ");

            migrationBuilder.Sql(@"
                CREATE SPATIAL INDEX IX_CinemaRoom_Location
                ON CinemaRooms(Location)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IX_CinemaRoom_Location
                ON CinemaRooms
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE CinemaRooms
                ALTER COLUMN Location geometry
            ");
        }
    }
}
