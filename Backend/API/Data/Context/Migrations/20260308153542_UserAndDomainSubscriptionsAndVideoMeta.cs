using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.API.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class UserAndDomainSubscriptionsAndVideoMeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainSigned");

            migrationBuilder.DropTable(
                name: "UserSubscription");

            migrationBuilder.CreateTable(
                name: "DomainSubscription",
                columns: table => new
                {
                    SubscriberId = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscribedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notification = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainSubscription", x => new { x.SubscriberId, x.DomainId });
                    table.ForeignKey(
                        name: "FK_DomainSubscription_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainSubscription_Users_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscribtion",
                columns: table => new
                {
                    FollowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notification = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscribtion", x => new { x.FollowerId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserSubscribtion_Users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscribtion_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainSubscription_DomainId",
                table: "DomainSubscription",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscribtion_UserId",
                table: "UserSubscribtion",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainSubscription");

            migrationBuilder.DropTable(
                name: "UserSubscribtion");

            migrationBuilder.CreateTable(
                name: "DomainSigned",
                columns: table => new
                {
                    SignedDomainsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignedUsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainSigned", x => new { x.SignedDomainsId, x.SignedUsersId });
                    table.ForeignKey(
                        name: "FK_DomainSigned_Domains_SignedDomainsId",
                        column: x => x.SignedDomainsId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainSigned_Users_SignedUsersId",
                        column: x => x.SignedUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscription",
                columns: table => new
                {
                    SubscribesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscription", x => new { x.SubscribesId, x.SubscriptionsId });
                    table.ForeignKey(
                        name: "FK_UserSubscription_Users_SubscribesId",
                        column: x => x.SubscribesId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscription_Users_SubscriptionsId",
                        column: x => x.SubscriptionsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainSigned_SignedUsersId",
                table: "DomainSigned",
                column: "SignedUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_SubscriptionsId",
                table: "UserSubscription",
                column: "SubscriptionsId");
        }
    }
}
