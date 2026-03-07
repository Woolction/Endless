using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.API.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class NewColumnsInDomainUserContentComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DomainUser_Domains_DomainsId",
                table: "DomainUser");

            migrationBuilder.DropForeignKey(
                name: "FK_DomainUser_Users_OwnersId",
                table: "DomainUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DomainUser",
                table: "DomainUser");

            migrationBuilder.RenameTable(
                name: "DomainUser",
                newName: "DomainsOwners");

            migrationBuilder.RenameColumn(
                name: "FollowersCount",
                table: "Domains",
                newName: "TotalViews");

            migrationBuilder.RenameIndex(
                name: "IX_DomainUser_OwnersId",
                table: "DomainsOwners",
                newName: "IX_DomainsOwners_OwnersId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistryData",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "TotalLikes",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "CraetedDate",
                table: "Domains",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "TotalLikes",
                table: "Domains",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Contents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "DizLikeCount",
                table: "Contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "DomainId",
                table: "Contents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "LikeCount",
                table: "Contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicationDate",
                table: "Contents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

            migrationBuilder.AddColumn<long>(
                name: "ViewsCount",
                table: "Contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "contentType",
                table: "Contents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DomainsOwners",
                table: "DomainsOwners",
                columns: new[] { "DomainsId", "OwnersId" });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LikeCount = table.Column<long>(type: "bigint", nullable: false),
                    DizLikeCount = table.Column<long>(type: "bigint", nullable: false),
                    ViewsCount = table.Column<long>(type: "bigint", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentatorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comment_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comment_Users_CommentatorId",
                        column: x => x.CommentatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Contents_DomainId",
                table: "Contents",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_UserId",
                table: "Contents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_UserId1",
                table: "Contents",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_CommentatorId",
                table: "Comment",
                column: "CommentatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ContentId",
                table: "Comment",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainSigned_SignedUsersId",
                table: "DomainSigned",
                column: "SignedUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_SubscriptionsId",
                table: "UserSubscription",
                column: "SubscriptionsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Domains_DomainId",
                table: "Contents",
                column: "DomainId",
                principalTable: "Domains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_DomainsOwners_Domains_DomainsId",
                table: "DomainsOwners",
                column: "DomainsId",
                principalTable: "Domains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DomainsOwners_Users_OwnersId",
                table: "DomainsOwners",
                column: "OwnersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Domains_DomainId",
                table: "Contents");

            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Users_UserId",
                table: "Contents");

            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Users_UserId1",
                table: "Contents");

            migrationBuilder.DropForeignKey(
                name: "FK_DomainsOwners_Domains_DomainsId",
                table: "DomainsOwners");

            migrationBuilder.DropForeignKey(
                name: "FK_DomainsOwners_Users_OwnersId",
                table: "DomainsOwners");

            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "DomainSigned");

            migrationBuilder.DropTable(
                name: "UserSubscription");

            migrationBuilder.DropIndex(
                name: "IX_Contents_DomainId",
                table: "Contents");

            migrationBuilder.DropIndex(
                name: "IX_Contents_UserId",
                table: "Contents");

            migrationBuilder.DropIndex(
                name: "IX_Contents_UserId1",
                table: "Contents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DomainsOwners",
                table: "DomainsOwners");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegistryData",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalLikes",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CraetedDate",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "TotalLikes",
                table: "Domains");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "DizLikeCount",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "DomainId",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "PublicationDate",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "ViewsCount",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "contentType",
                table: "Contents");

            migrationBuilder.RenameTable(
                name: "DomainsOwners",
                newName: "DomainUser");

            migrationBuilder.RenameColumn(
                name: "TotalViews",
                table: "Domains",
                newName: "FollowersCount");

            migrationBuilder.RenameIndex(
                name: "IX_DomainsOwners_OwnersId",
                table: "DomainUser",
                newName: "IX_DomainUser_OwnersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DomainUser",
                table: "DomainUser",
                columns: new[] { "DomainsId", "OwnersId" });

            migrationBuilder.AddForeignKey(
                name: "FK_DomainUser_Domains_DomainsId",
                table: "DomainUser",
                column: "DomainsId",
                principalTable: "Domains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DomainUser_Users_OwnersId",
                table: "DomainUser",
                column: "OwnersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
