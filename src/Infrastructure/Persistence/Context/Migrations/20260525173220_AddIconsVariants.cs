using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddIconsVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarPhotoUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarPhotoUrl",
                table: "Channels");

            migrationBuilder.RenameColumn(
                name: "BaseUrl",
                table: "VideoMetas",
                newName: "PhotoBase");

            migrationBuilder.CreateTable(
                name: "ChannelMetas",
                columns: table => new
                {
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    IconBase = table.Column<string>(type: "text", nullable: false),
                    Small = table.Column<string>(type: "text", nullable: false),
                    Medium = table.Column<string>(type: "text", nullable: true),
                    Large = table.Column<string>(type: "text", nullable: true),
                    R = table.Column<int>(type: "integer", nullable: false),
                    G = table.Column<int>(type: "integer", nullable: false),
                    B = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelMetas", x => x.ChannelId);
                    table.ForeignKey(
                        name: "FK_ChannelMetas_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMetas",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IconBase = table.Column<string>(type: "text", nullable: false),
                    Small = table.Column<string>(type: "text", nullable: false),
                    Medium = table.Column<string>(type: "text", nullable: true),
                    Large = table.Column<string>(type: "text", nullable: true),
                    R = table.Column<int>(type: "integer", nullable: false),
                    G = table.Column<int>(type: "integer", nullable: false),
                    B = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMetas", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserMetas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelMetas");

            migrationBuilder.DropTable(
                name: "UserMetas");

            migrationBuilder.RenameColumn(
                name: "PhotoBase",
                table: "VideoMetas",
                newName: "BaseUrl");

            migrationBuilder.AddColumn<string>(
                name: "AvatarPhotoUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarPhotoUrl",
                table: "Channels",
                type: "text",
                nullable: true);
        }
    }
}
