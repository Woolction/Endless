using Microsoft.EntityFrameworkCore.Migrations;
using Domain.Entities;

#nullable disable

namespace Infrastructure.Context.Migrations
{
    /// <inheritdoc />
    public partial class GenreInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "UserVectors");

            migrationBuilder.DropColumn(
                name: "VectorsCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "ContentVectors");

            migrationBuilder.DropColumn(
                name: "VectorsCount",
                table: "Contents");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Genres",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GenreInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreInfos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenreInfos");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Genres");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "UserVectors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "VectorsCount",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "ContentVectors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "VectorsCount",
                table: "Contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        public static implicit operator GenreInfo(Domain.Entities.GenreInfo v)
        {
            throw new NotImplementedException();
        }
    }
}
