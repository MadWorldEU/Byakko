using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace MadWorldEU.Byakko.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleteAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "DeletedAt",
                table: "Assets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "ExpiresAt",
                table: "Assets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Assets");
        }
    }
}
