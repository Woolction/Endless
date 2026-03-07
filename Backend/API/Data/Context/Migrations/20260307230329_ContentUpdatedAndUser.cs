using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.API.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class ContentUpdatedAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Users_UserId",
                table: "Contents");

            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Users_UserId1",
                table: "Contents");

            migrationBuilder.DropIndex(
                name: "IX_Contents_UserId",
                table: "Contents");

            migrationBuilder.DropIndex(
                name: "IX_Contents_UserId1",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Contents");

            migrationBuilder.RenameColumn(
                name: "contentType",
                table: "Contents",
                newName: "ContentType");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Contents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "LikedContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikedContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LikedContent_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LikedContent_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedContent_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedContent_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contents_CreatorId",
                table: "Contents",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_LikedContent_ContentId",
                table: "LikedContent",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_LikedContent_OwnerId",
                table: "LikedContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedContent_ContentId",
                table: "SavedContent",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedContent_OwnerId",
                table: "SavedContent",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Users_CreatorId",
                table: "Contents",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Users_CreatorId",
                table: "Contents");

            migrationBuilder.DropTable(
                name: "LikedContent");

            migrationBuilder.DropTable(
                name: "SavedContent");

            migrationBuilder.DropIndex(
                name: "IX_Contents_CreatorId",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Contents");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Contents",
                newName: "contentType");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Contents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Contents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contents_UserId",
                table: "Contents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_UserId1",
                table: "Contents",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Users_UserId",
                table: "Contents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Users_UserId1",
                table: "Contents",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
