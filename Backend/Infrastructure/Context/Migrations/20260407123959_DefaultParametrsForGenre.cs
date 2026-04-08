using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Context.Migrations
{
    /// <inheritdoc />
    public partial class DefaultParametrsForGenre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInterationContents");

            migrationBuilder.CreateTable(
                name: "UserInteractionContents",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    WatchTimeSeconds = table.Column<int>(type: "integer", nullable: false),
                    Liked = table.Column<bool>(type: "boolean", nullable: false),
                    Saved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInteractionContents", x => new { x.UserId, x.ContentId });
                    table.ForeignKey(
                        name: "FK_UserInteractionContents_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInteractionContents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GenreInfos",
                columns: new[] { "Id", "Count" },
                values: new object[] { new Guid("018f47ac-8b72-7abc-8def-1234567890ab"), 6 });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Name", "Order" },
                values: new object[,]
                {
                    { new Guid("018f47ac-8b72-7c2f-b8d1-9f3c2e7a6d11"), "Vlog", 0 },
                    { new Guid("018f47ac-8b72-7c30-a2f4-6b1d9c8e2a55"), "Gaming", 1 },
                    { new Guid("018f47ac-8b72-7c31-91aa-3e7f5b2c4d88"), "Tutorial", 2 },
                    { new Guid("018f47ac-8b72-7c32-b7c1-0a9d6e4f2b33"), "Review", 3 },
                    { new Guid("018f47ac-8b72-7c33-8d2e-5c1a9b7f3e66"), "Education", 4 },
                    { new Guid("018f47ac-8b72-7c34-a9f0-2d6c8b1e4a99"), "Tech", 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInteractionContents_ContentId",
                table: "UserInteractionContents",
                column: "ContentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInteractionContents");

            migrationBuilder.DeleteData(
                table: "GenreInfos",
                keyColumn: "Id",
                keyValue: new Guid("018f47ac-8b72-7abc-8def-1234567890ab"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("018f47ac-8b72-7c2f-b8d1-9f3c2e7a6d11"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("018f47ac-8b72-7c30-a2f4-6b1d9c8e2a55"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("018f47ac-8b72-7c31-91aa-3e7f5b2c4d88"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("018f47ac-8b72-7c32-b7c1-0a9d6e4f2b33"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("018f47ac-8b72-7c33-8d2e-5c1a9b7f3e66"));

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: new Guid("018f47ac-8b72-7c34-a9f0-2d6c8b1e4a99"));

            migrationBuilder.CreateTable(
                name: "UserInterationContents",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Liked = table.Column<bool>(type: "boolean", nullable: false),
                    Saved = table.Column<bool>(type: "boolean", nullable: false),
                    WatchTimeSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInterationContents", x => new { x.UserId, x.ContentId });
                    table.ForeignKey(
                        name: "FK_UserInterationContents_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInterationContents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInterationContents_ContentId",
                table: "UserInterationContents",
                column: "ContentId");
        }
    }
}
