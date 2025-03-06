using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tournaments.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Gamertag = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Gamertag);
                });

            migrationBuilder.CreateTable(
                name: "Tournament",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParentTournamentName = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournament", x => x.Name);
                    table.ForeignKey(
                        name: "FK_Tournament_Tournament_ParentTournamentName",
                        column: x => x.ParentTournamentName,
                        principalTable: "Tournament",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateTable(
                name: "Registration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlayerGamertag = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registration_Player_PlayerGamertag",
                        column: x => x.PlayerGamertag,
                        principalTable: "Player",
                        principalColumn: "Gamertag",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Registration_Tournament_TournamentName",
                        column: x => x.TournamentName,
                        principalTable: "Tournament",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Registration_PlayerGamertag",
                table: "Registration",
                column: "PlayerGamertag");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_TournamentName",
                table: "Registration",
                column: "TournamentName");

            migrationBuilder.CreateIndex(
                name: "IX_Tournament_ParentTournamentName",
                table: "Tournament",
                column: "ParentTournamentName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Registration");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "Tournament");
        }
    }
}
