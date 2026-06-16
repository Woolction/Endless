using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddImageAndVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "VideoMetas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "UserMetas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "ChannelMetas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    R = table.Column<int>(type: "integer", nullable: false),
                    G = table.Column<int>(type: "integer", nullable: false),
                    B = table.Column<int>(type: "integer", nullable: false),
                    BaseUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageVariants",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageVariants", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_ImageVariants_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoMetas_ImageId",
                table: "VideoMetas",
                column: "ImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMetas_ImageId",
                table: "UserMetas",
                column: "ImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelMetas_ImageId",
                table: "ChannelMetas",
                column: "ImageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelMetas_Images_ImageId",
                table: "ChannelMetas",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMetas_Images_ImageId",
                table: "UserMetas",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoMetas_Images_ImageId",
                table: "VideoMetas",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelMetas_Images_ImageId",
                table: "ChannelMetas");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMetas_Images_ImageId",
                table: "UserMetas");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoMetas_Images_ImageId",
                table: "VideoMetas");

            migrationBuilder.DropTable(
                name: "ImageVariants");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropIndex(
                name: "IX_VideoMetas_ImageId",
                table: "VideoMetas");

            migrationBuilder.DropIndex(
                name: "IX_UserMetas_ImageId",
                table: "UserMetas");

            migrationBuilder.DropIndex(
                name: "IX_ChannelMetas_ImageId",
                table: "ChannelMetas");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "UserMetas");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "ChannelMetas");
        }
    }
}
