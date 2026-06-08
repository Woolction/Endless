using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddPathsToVideoMeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentUrl",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "PreviewPhotoUrl",
                table: "Contents");

            migrationBuilder.AddColumn<int>(
                name: "ColorB",
                table: "VideoMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ColorG",
                table: "VideoMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ColorR",
                table: "VideoMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "VideoMetas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "VideoMetas",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorB",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "ColorG",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "ColorR",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "VideoMetas");

            migrationBuilder.AddColumn<string>(
                name: "ContentUrl",
                table: "Contents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviewPhotoUrl",
                table: "Contents",
                type: "text",
                nullable: true);
        }
    }
}
