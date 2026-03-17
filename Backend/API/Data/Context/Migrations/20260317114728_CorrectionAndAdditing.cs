using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.API.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class CorrectionAndAdditing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LikedContents_Users_OwnerId",
                table: "LikedContents");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedContents_Users_OwnerId",
                table: "SavedContents");

            migrationBuilder.DropTable(
                name: "UserSubscribtions");

            migrationBuilder.RenameColumn(
                name: "DomainSubscriptionsCount",
                table: "Users",
                newName: "SubscripedDomainsCount");

            migrationBuilder.RenameColumn(
                name: "RegistryDate",
                table: "SavedContents",
                newName: "SavedDate");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "SavedContents",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "RegistryDate",
                table: "LikedContents",
                newName: "LikedDate");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "LikedContents",
                newName: "UserId");

            migrationBuilder.AddColumn<long>(
                name: "CommentsLikesCount",
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
                name: "LikedCommentsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "LikedComments",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LikedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikedComments", x => new { x.UserId, x.CommentId });
                    table.ForeignKey(
                        name: "FK_LikedComments_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LikedComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFollowings",
                columns: table => new
                {
                    FollowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notification = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollowings", x => new { x.FollowerId, x.FollowedUserId });
                    table.ForeignKey(
                        name: "FK_UserFollowings_Users_FollowedUserId",
                        column: x => x.FollowedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFollowings_Users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LikedComments_CommentId",
                table: "LikedComments",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollowings_FollowedUserId",
                table: "UserFollowings",
                column: "FollowedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LikedContents_Users_UserId",
                table: "LikedContents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedContents_Users_UserId",
                table: "SavedContents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LikedContents_Users_UserId",
                table: "LikedContents");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedContents_Users_UserId",
                table: "SavedContents");

            migrationBuilder.DropTable(
                name: "LikedComments");

            migrationBuilder.DropTable(
                name: "UserFollowings");

            migrationBuilder.DropColumn(
                name: "CommentsLikesCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContentsLikesCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContentsSavesCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LikedCommentsCount",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "SubscripedDomainsCount",
                table: "Users",
                newName: "DomainSubscriptionsCount");

            migrationBuilder.RenameColumn(
                name: "SavedDate",
                table: "SavedContents",
                newName: "RegistryDate");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "SavedContents",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "LikedDate",
                table: "LikedContents",
                newName: "RegistryDate");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "LikedContents",
                newName: "OwnerId");

            migrationBuilder.CreateTable(
                name: "UserSubscribtions",
                columns: table => new
                {
                    FollowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notification = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscribtions", x => new { x.FollowerId, x.FollowedUserId });
                    table.ForeignKey(
                        name: "FK_UserSubscribtions_Users_FollowedUserId",
                        column: x => x.FollowedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscribtions_Users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscribtions_FollowedUserId",
                table: "UserSubscribtions",
                column: "FollowedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LikedContents_Users_OwnerId",
                table: "LikedContents",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedContents_Users_OwnerId",
                table: "SavedContents",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
