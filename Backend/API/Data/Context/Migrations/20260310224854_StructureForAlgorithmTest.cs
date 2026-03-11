using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.API.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class StructureForAlgorithmTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentGenreVectors_Contents_ContentId",
                table: "ContentGenreVectors");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentGenreVectors_Genres_GenreId",
                table: "ContentGenreVectors");

            migrationBuilder.DropForeignKey(
                name: "FK_GenreVectors_Genres_GenreId",
                table: "GenreVectors");

            migrationBuilder.DropForeignKey(
                name: "FK_GenreVectors_Users_UserId",
                table: "GenreVectors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GenreVectors",
                table: "GenreVectors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentGenreVectors",
                table: "ContentGenreVectors");

            migrationBuilder.DropColumn(
                name: "WatchTime",
                table: "UserInterationContents");

            migrationBuilder.RenameTable(
                name: "GenreVectors",
                newName: "UserVectors");

            migrationBuilder.RenameTable(
                name: "ContentGenreVectors",
                newName: "ContentVectors");

            migrationBuilder.RenameColumn(
                name: "DizLikeCount",
                table: "Contents",
                newName: "VectorsCount");

            migrationBuilder.RenameColumn(
                name: "ContentSaversCount",
                table: "Contents",
                newName: "SavesCount");

            migrationBuilder.RenameColumn(
                name: "ContentLikersCount",
                table: "Contents",
                newName: "LikesCount");

            migrationBuilder.RenameColumn(
                name: "AverageWatchTime",
                table: "Contents",
                newName: "TrendingScore");

            migrationBuilder.RenameIndex(
                name: "IX_GenreVectors_GenreId",
                table: "UserVectors",
                newName: "IX_UserVectors_GenreId");

            migrationBuilder.RenameIndex(
                name: "IX_ContentGenreVectors_GenreId",
                table: "ContentVectors",
                newName: "IX_ContentVectors_GenreId");

            migrationBuilder.AddColumn<float>(
                name: "AverageWatchRatio",
                table: "VideoMetas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "AverageWatchTimeSeconds",
                table: "VideoMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "VideoMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "UserInterationsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "VectorsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "WatchTimeSeconds",
                table: "UserInterationContents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "DizLikesCount",
                table: "Contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserVectors",
                table: "UserVectors",
                columns: new[] { "UserId", "GenreId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentVectors",
                table: "ContentVectors",
                columns: new[] { "ContentId", "GenreId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ContentVectors_Contents_ContentId",
                table: "ContentVectors",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentVectors_Genres_GenreId",
                table: "ContentVectors",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVectors_Genres_GenreId",
                table: "UserVectors",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVectors_Users_UserId",
                table: "UserVectors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentVectors_Contents_ContentId",
                table: "ContentVectors");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentVectors_Genres_GenreId",
                table: "ContentVectors");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVectors_Genres_GenreId",
                table: "UserVectors");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVectors_Users_UserId",
                table: "UserVectors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserVectors",
                table: "UserVectors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentVectors",
                table: "ContentVectors");

            migrationBuilder.DropColumn(
                name: "AverageWatchRatio",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "AverageWatchTimeSeconds",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "UserInterationsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VectorsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WatchTimeSeconds",
                table: "UserInterationContents");

            migrationBuilder.DropColumn(
                name: "DizLikesCount",
                table: "Contents");

            migrationBuilder.RenameTable(
                name: "UserVectors",
                newName: "GenreVectors");

            migrationBuilder.RenameTable(
                name: "ContentVectors",
                newName: "ContentGenreVectors");

            migrationBuilder.RenameColumn(
                name: "VectorsCount",
                table: "Contents",
                newName: "DizLikeCount");

            migrationBuilder.RenameColumn(
                name: "TrendingScore",
                table: "Contents",
                newName: "AverageWatchTime");

            migrationBuilder.RenameColumn(
                name: "SavesCount",
                table: "Contents",
                newName: "ContentSaversCount");

            migrationBuilder.RenameColumn(
                name: "LikesCount",
                table: "Contents",
                newName: "ContentLikersCount");

            migrationBuilder.RenameIndex(
                name: "IX_UserVectors_GenreId",
                table: "GenreVectors",
                newName: "IX_GenreVectors_GenreId");

            migrationBuilder.RenameIndex(
                name: "IX_ContentVectors_GenreId",
                table: "ContentGenreVectors",
                newName: "IX_ContentGenreVectors_GenreId");

            migrationBuilder.AddColumn<float>(
                name: "WatchTime",
                table: "UserInterationContents",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GenreVectors",
                table: "GenreVectors",
                columns: new[] { "UserId", "GenreId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentGenreVectors",
                table: "ContentGenreVectors",
                columns: new[] { "ContentId", "GenreId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ContentGenreVectors_Contents_ContentId",
                table: "ContentGenreVectors",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentGenreVectors_Genres_GenreId",
                table: "ContentGenreVectors",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GenreVectors_Genres_GenreId",
                table: "GenreVectors",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GenreVectors_Users_UserId",
                table: "GenreVectors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
