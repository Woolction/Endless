using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Context.Migrations
{
    /// <inheritdoc />
    public partial class DeleteColorWithColumnsName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ColorR",
                table: "VideoMetas",
                newName: "R");

            migrationBuilder.RenameColumn(
                name: "ColorG",
                table: "VideoMetas",
                newName: "G");

            migrationBuilder.RenameColumn(
                name: "ColorB",
                table: "VideoMetas",
                newName: "B");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "R",
                table: "VideoMetas",
                newName: "ColorR");

            migrationBuilder.RenameColumn(
                name: "G",
                table: "VideoMetas",
                newName: "ColorG");

            migrationBuilder.RenameColumn(
                name: "B",
                table: "VideoMetas",
                newName: "ColorB");
        }
    }
}
