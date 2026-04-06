using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Context.Migrations
{
    /// <inheritdoc />
    public partial class RenamedDomainToChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Domains_DomainId",
                table: "Contents");

            migrationBuilder.DropTable(
                name: "DomainOwners");

            migrationBuilder.DropTable(
                name: "DomainSubscriptions");

            migrationBuilder.DropTable(
                name: "Domains");

            migrationBuilder.RenameColumn(
                name: "DomainId",
                table: "Contents",
                newName: "ChannelId");

            migrationBuilder.RenameIndex(
                name: "IX_Contents_DomainId",
                table: "Contents",
                newName: "IX_Contents_ChannelId");

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvatarPhotoUrl = table.Column<string>(type: "text", nullable: true),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    TotalLikes = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChannelOwners",
                columns: table => new
                {
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerRole = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelOwners", x => new { x.OwnerId, x.ChannelId });
                    table.ForeignKey(
                        name: "FK_ChannelOwners_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelOwners_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelSubscriptions",
                columns: table => new
                {
                    SubscriberId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscribedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notification = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSubscriptions", x => new { x.SubscriberId, x.ChannelId });
                    table.ForeignKey(
                        name: "FK_ChannelSubscriptions_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelSubscriptions_Users_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelOwners_ChannelId",
                table: "ChannelOwners",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_Name",
                table: "Channels",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Channels_Slug",
                table: "Channels",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSubscriptions_ChannelId",
                table: "ChannelSubscriptions",
                column: "ChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Channels_ChannelId",
                table: "Contents",
                column: "ChannelId",
                principalTable: "Channels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Channels_ChannelId",
                table: "Contents");

            migrationBuilder.DropTable(
                name: "ChannelOwners");

            migrationBuilder.DropTable(
                name: "ChannelSubscriptions");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "Contents",
                newName: "DomainId");

            migrationBuilder.RenameIndex(
                name: "IX_Contents_ChannelId",
                table: "Contents",
                newName: "IX_Contents_DomainId");

            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AvatarPhotoUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    TotalLikes = table.Column<long>(type: "bigint", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainOwners",
                columns: table => new
                {
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerRole = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainOwners", x => new { x.OwnerId, x.DomainId });
                    table.ForeignKey(
                        name: "FK_DomainOwners_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainOwners_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DomainSubscriptions",
                columns: table => new
                {
                    SubscriberId = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notification = table.Column<bool>(type: "boolean", nullable: false),
                    SubscribedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainSubscriptions", x => new { x.SubscriberId, x.DomainId });
                    table.ForeignKey(
                        name: "FK_DomainSubscriptions_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainSubscriptions_Users_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainOwners_DomainId",
                table: "DomainOwners",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_Name",
                table: "Domains",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Domains_Slug",
                table: "Domains",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainSubscriptions_DomainId",
                table: "DomainSubscriptions",
                column: "DomainId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Domains_DomainId",
                table: "Contents",
                column: "DomainId",
                principalTable: "Domains",
                principalColumn: "Id");
        }
    }
}
