using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class PreparationForRecommendationAlgorithm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscribtions_Users_UserId",
                table: "UserSubscribtions");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "VideoMetas");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserSubscribtions",
                newName: "FollowedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscribtions_UserId",
                table: "UserSubscribtions",
                newName: "IX_UserSubscribtions_FollowedUserId");

            migrationBuilder.AddColumn<float>(
                name: "AverageWatchTime",
                table: "Contents",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserInterationContents",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    WatchTime = table.Column<float>(type: "real", nullable: false),
                    Liked = table.Column<bool>(type: "boolean", nullable: false),
                    Saved = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ContentGenreVectors",
                columns: table => new
                {
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenreId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorVector = table.Column<float>(type: "real", nullable: false),
                    AudienceVector = table.Column<float>(type: "real", nullable: false),
                    FinalVector = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentGenreVectors", x => new { x.ContentId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_ContentGenreVectors_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentGenreVectors_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenreVectors",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenreId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreVectors", x => new { x.UserId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_GenreVectors_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenreVectors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentGenreVectors_GenreId",
                table: "ContentGenreVectors",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Name",
                table: "Genres",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_GenreVectors_GenreId",
                table: "GenreVectors",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterationContents_ContentId",
                table: "UserInterationContents",
                column: "ContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscribtions_Users_FollowedUserId",
                table: "UserSubscribtions",
                column: "FollowedUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscribtions_Users_FollowedUserId",
                table: "UserSubscribtions");

            migrationBuilder.DropTable(
                name: "ContentGenreVectors");

            migrationBuilder.DropTable(
                name: "GenreVectors");

            migrationBuilder.DropTable(
                name: "UserInterationContents");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropColumn(
                name: "AverageWatchTime",
                table: "Contents");

            migrationBuilder.RenameColumn(
                name: "FollowedUserId",
                table: "UserSubscribtions",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscribtions_FollowedUserId",
                table: "UserSubscribtions",
                newName: "IX_UserSubscribtions_UserId");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "VideoMetas",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscribtions_Users_UserId",
                table: "UserSubscribtions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
