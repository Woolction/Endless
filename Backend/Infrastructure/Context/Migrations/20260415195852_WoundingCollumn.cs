using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Context.Migrations
{
    /// <inheritdoc />
    public partial class WoundingCollumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWound",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWound",
                table: "Contents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWound",
                table: "Channels",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWound",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsWound",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "IsWound",
                table: "Channels");
        }
    }
}
