using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesAPI.Migrations
{
    /// <inheritdoc />
    public partial class CinemaRoomsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CinemaRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CinemaRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoviesCinemaRooms",
                columns: table => new
                {
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    CinemaRoomId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviesCinemaRooms", x => new { x.MovieId, x.CinemaRoomId });
                    table.ForeignKey(
                        name: "FK_MoviesCinemaRooms_CinemaRooms_CinemaRoomId",
                        column: x => x.CinemaRoomId,
                        principalTable: "CinemaRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MoviesCinemaRooms_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoviesCinemaRooms_CinemaRoomId",
                table: "MoviesCinemaRooms",
                column: "CinemaRoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoviesCinemaRooms");

            migrationBuilder.DropTable(
                name: "CinemaRooms");
        }
    }
}
