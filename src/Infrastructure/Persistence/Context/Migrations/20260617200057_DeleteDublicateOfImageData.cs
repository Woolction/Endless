using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Context.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDublicateOfImageData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "B",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "G",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "Large",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "Medium",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "PhotoBase",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "R",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "Small",
                table: "VideoMetas");

            migrationBuilder.DropColumn(
                name: "B",
                table: "UserMetas");

            migrationBuilder.DropColumn(
                name: "G",
                table: "UserMetas");

            migrationBuilder.DropColumn(
                name: "IconBase",
                table: "UserMetas");

            migrationBuilder.DropColumn(
                name: "Large",
                table: "UserMetas");

            migrationBuilder.DropColumn(
                name: "Medium",
                table: "UserMetas");

            migrationBuilder.DropColumn(
                name: "R",
                table: "UserMetas");

            migrationBuilder.DropColumn(
                name: "Small",
                table: "UserMetas");

            migrationBuilder.DropColumn(
                name: "B",
                table: "ChannelMetas");

            migrationBuilder.DropColumn(
                name: "G",
                table: "ChannelMetas");

            migrationBuilder.DropColumn(
                name: "IconBase",
                table: "ChannelMetas");

            migrationBuilder.DropColumn(
                name: "Large",
                table: "ChannelMetas");

            migrationBuilder.DropColumn(
                name: "Medium",
                table: "ChannelMetas");

            migrationBuilder.DropColumn(
                name: "R",
                table: "ChannelMetas");

            migrationBuilder.DropColumn(
                name: "Small",
                table: "ChannelMetas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "B",
                table: "VideoMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "G",
                table: "VideoMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Large",
                table: "VideoMetas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Medium",
                table: "VideoMetas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoBase",
                table: "VideoMetas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "R",
                table: "VideoMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Small",
                table: "VideoMetas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "B",
                table: "UserMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "G",
                table: "UserMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IconBase",
                table: "UserMetas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Large",
                table: "UserMetas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Medium",
                table: "UserMetas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "R",
                table: "UserMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Small",
                table: "UserMetas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "B",
                table: "ChannelMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "G",
                table: "ChannelMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IconBase",
                table: "ChannelMetas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Large",
                table: "ChannelMetas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Medium",
                table: "ChannelMetas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "R",
                table: "ChannelMetas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Small",
                table: "ChannelMetas",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
