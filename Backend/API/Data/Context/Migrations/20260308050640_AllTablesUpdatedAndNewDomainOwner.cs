using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.API.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class AllTablesUpdatedAndNewDomainOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Contents_ContentId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Users_CommentatorId",
                table: "Comment");

            migrationBuilder.DropForeignKey(
                name: "FK_LikedContent_Contents_ContentId",
                table: "LikedContent");

            migrationBuilder.DropForeignKey(
                name: "FK_LikedContent_Users_OwnerId",
                table: "LikedContent");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedContent_Contents_ContentId",
                table: "SavedContent");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedContent_Users_OwnerId",
                table: "SavedContent");

            migrationBuilder.DropTable(
                name: "DomainsOwners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedContent",
                table: "SavedContent");

            migrationBuilder.DropIndex(
                name: "IX_SavedContent_OwnerId",
                table: "SavedContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LikedContent",
                table: "LikedContent");

            migrationBuilder.DropIndex(
                name: "IX_LikedContent_OwnerId",
                table: "LikedContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Comment",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SavedContent");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "LikedContent");

            migrationBuilder.RenameTable(
                name: "SavedContent",
                newName: "SavedContents");

            migrationBuilder.RenameTable(
                name: "LikedContent",
                newName: "LikedContents");

            migrationBuilder.RenameTable(
                name: "Comment",
                newName: "Comments");

            migrationBuilder.RenameColumn(
                name: "CraetedDate",
                table: "Domains",
                newName: "CreatedDate");

            migrationBuilder.RenameIndex(
                name: "IX_SavedContent_ContentId",
                table: "SavedContents",
                newName: "IX_SavedContents_ContentId");

            migrationBuilder.RenameIndex(
                name: "IX_LikedContent_ContentId",
                table: "LikedContents",
                newName: "IX_LikedContents_ContentId");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_ContentId",
                table: "Comments",
                newName: "IX_Comments_ContentId");

            migrationBuilder.RenameIndex(
                name: "IX_Comment_CommentatorId",
                table: "Comments",
                newName: "IX_Comments_CommentatorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedContents",
                table: "SavedContents",
                columns: new[] { "OwnerId", "ContentId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_LikedContents",
                table: "LikedContents",
                columns: new[] { "OwnerId", "ContentId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comments",
                table: "Comments",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DomainOwners",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerRole = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainOwners", x => new { x.UserId, x.DomainId });
                    table.ForeignKey(
                        name: "FK_DomainOwners_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainOwners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainOwners_DomainId",
                table: "DomainOwners",
                column: "DomainId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Contents_ContentId",
                table: "Comments",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_CommentatorId",
                table: "Comments",
                column: "CommentatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LikedContents_Contents_ContentId",
                table: "LikedContents",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LikedContents_Users_OwnerId",
                table: "LikedContents",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedContents_Contents_ContentId",
                table: "SavedContents",
                column: "ContentId",
                principalTable: "Contents",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Contents_ContentId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_CommentatorId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_LikedContents_Contents_ContentId",
                table: "LikedContents");

            migrationBuilder.DropForeignKey(
                name: "FK_LikedContents_Users_OwnerId",
                table: "LikedContents");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedContents_Contents_ContentId",
                table: "SavedContents");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedContents_Users_OwnerId",
                table: "SavedContents");

            migrationBuilder.DropTable(
                name: "DomainOwners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedContents",
                table: "SavedContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LikedContents",
                table: "LikedContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Comments",
                table: "Comments");

            migrationBuilder.RenameTable(
                name: "SavedContents",
                newName: "SavedContent");

            migrationBuilder.RenameTable(
                name: "LikedContents",
                newName: "LikedContent");

            migrationBuilder.RenameTable(
                name: "Comments",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Domains",
                newName: "CraetedDate");

            migrationBuilder.RenameIndex(
                name: "IX_SavedContents_ContentId",
                table: "SavedContent",
                newName: "IX_SavedContent_ContentId");

            migrationBuilder.RenameIndex(
                name: "IX_LikedContents_ContentId",
                table: "LikedContent",
                newName: "IX_LikedContent_ContentId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_ContentId",
                table: "Comment",
                newName: "IX_Comment_ContentId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_CommentatorId",
                table: "Comment",
                newName: "IX_Comment_CommentatorId");

            migrationBuilder.AddColumn<long>(
                name: "LikeCount",
                table: "Contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SavedContent",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "LikedContent",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedContent",
                table: "SavedContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LikedContent",
                table: "LikedContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comment",
                table: "Comment",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DomainsOwners",
                columns: table => new
                {
                    DomainsId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainsOwners", x => new { x.DomainsId, x.OwnersId });
                    table.ForeignKey(
                        name: "FK_DomainsOwners_Domains_DomainsId",
                        column: x => x.DomainsId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DomainsOwners_Users_OwnersId",
                        column: x => x.OwnersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedContent_OwnerId",
                table: "SavedContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LikedContent_OwnerId",
                table: "LikedContent",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainsOwners_OwnersId",
                table: "DomainsOwners",
                column: "OwnersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Contents_ContentId",
                table: "Comment",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Users_CommentatorId",
                table: "Comment",
                column: "CommentatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LikedContent_Contents_ContentId",
                table: "LikedContent",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LikedContent_Users_OwnerId",
                table: "LikedContent",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedContent_Contents_ContentId",
                table: "SavedContent",
                column: "ContentId",
                principalTable: "Contents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedContent_Users_OwnerId",
                table: "SavedContent",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
