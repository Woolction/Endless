using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Context.Migrations
{
    /// <inheritdoc />
    public partial class DeletedThisCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CommentsLikesCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContentsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContentsLikesCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContentsSavesCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FollowersCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FollowingCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LikedCommentsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LikedContentsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OwnedDomainsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SavedContentsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SubscripedDomainsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContentsCount",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "OwnersCount",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "SubscribersCount",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "DizLikesCount",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "LikesCount",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "SavesCount",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "DizLikeCount",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "Comments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CommentsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CommentsLikesCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ContentsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ContentsLikesCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ContentsSavesCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FollowersCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FollowingCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "LikedCommentsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "LikedContentsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "OwnedDomainsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SavedContentsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SubscripedDomainsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ContentsCount",
                table: "Domains",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "OwnersCount",
                table: "Domains",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SubscribersCount",
                table: "Domains",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CommentsCount",
                table: "Contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DizLikesCount",
                table: "Contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "LikesCount",
                table: "Contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SavesCount",
                table: "Contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DizLikeCount",
                table: "Comments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "LikeCount",
                table: "Comments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
