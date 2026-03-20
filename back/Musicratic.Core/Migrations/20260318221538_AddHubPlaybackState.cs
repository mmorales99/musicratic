using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Musicratic.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddHubPlaybackState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlaybackState",
                table: "Hubs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlayingTrackId",
                table: "Hubs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PlayingTrackPositionSeconds",
                table: "Hubs",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaybackState",
                table: "Hubs");

            migrationBuilder.DropColumn(
                name: "PlayingTrackId",
                table: "Hubs");

            migrationBuilder.DropColumn(
                name: "PlayingTrackPositionSeconds",
                table: "Hubs");
        }
    }
}
