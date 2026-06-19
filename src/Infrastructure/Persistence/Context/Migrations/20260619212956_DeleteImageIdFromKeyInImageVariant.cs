using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Context.Migrations
{
    /// <inheritdoc />
    public partial class DeleteImageIdFromKeyInImageVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageVariants",
                table: "ImageVariants");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageVariants",
                table: "ImageVariants",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ImageVariants_ImageId",
                table: "ImageVariants",
                column: "ImageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageVariants",
                table: "ImageVariants");

            migrationBuilder.DropIndex(
                name: "IX_ImageVariants_ImageId",
                table: "ImageVariants");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageVariants",
                table: "ImageVariants",
                column: "ImageId");
        }
    }
}
