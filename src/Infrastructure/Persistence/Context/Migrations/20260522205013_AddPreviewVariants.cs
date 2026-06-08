using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviewVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhotoUrl",
                table: "VideoMetas",
                newName: "Small");

            migrationBuilder.AddColumn<string>(
                name: "BaseUrl",
                table: "VideoMetas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Large",
                table: "VideoMetas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Medium",
                table: "VideoMetas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseUrl",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "Large",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "Medium",
                table: "VideoMetas");

            migrationBuilder.RenameColumn(
                name: "Small",
                table: "VideoMetas",
                newName: "PhotoUrl");
        }
    }
}
