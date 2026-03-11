using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.API.Data.Context.Migrations
{
    /// <inheritdoc />
    public partial class TrigramsExtensionAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalViews = table.Column<long>(type: "bigint", nullable: false),
                    TotalLikes = table.Column<long>(type: "bigint", nullable: false),
                    SubscribersCount = table.Column<long>(type: "bigint", nullable: false),
                    OwnersCount = table.Column<long>(type: "bigint", nullable: false),
                    ContentsCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RefreshToken_Token = table.Column<string>(type: "text", nullable: true),
                    RefreshToken_ValidityPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegistryData = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PrivateType = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    TotalLikes = table.Column<long>(type: "bigint", nullable: false),
                    SavedContentsCount = table.Column<long>(type: "bigint", nullable: false),
                    LikedContentsCount = table.Column<long>(type: "bigint", nullable: false),
                    CommentsCount = table.Column<long>(type: "bigint", nullable: false),
                    ContentsCount = table.Column<long>(type: "bigint", nullable: false),
                    FollowersCount = table.Column<long>(type: "bigint", nullable: false),
                    FollowingCount = table.Column<long>(type: "bigint", nullable: false),
                    OwnedDomainsCount = table.Column<long>(type: "bigint", nullable: false),
                    DomainSubscriptionsCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    ContentUrl = table.Column<string>(type: "text", nullable: true),
                    PrewievPhotoUrl = table.Column<string>(type: "text", nullable: true),
                    DizLikeCount = table.Column<long>(type: "bigint", nullable: false),
                    ViewsCount = table.Column<long>(type: "bigint", nullable: false),
                    ContentSaversCount = table.Column<long>(type: "bigint", nullable: false),
                    ContentLikersCount = table.Column<long>(type: "bigint", nullable: false),
                    CommentsCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contents_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contents_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    SubscribedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notification = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "UserSubscribtions",
                columns: table => new
                {
                    FollowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notification = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscribtions", x => new { x.FollowerId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserSubscribtions_Users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscribtions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    PublicatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LikeCount = table.Column<long>(type: "bigint", nullable: false),
                    DizLikeCount = table.Column<long>(type: "bigint", nullable: false),
                    ViewsCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_CommentatorId",
                        column: x => x.CommentatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LikedContents",
                columns: table => new
                {
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikedContents", x => new { x.OwnerId, x.ContentId });
                    table.ForeignKey(
                        name: "FK_LikedContents_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LikedContents_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedContents",
                columns: table => new
                {
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedContents", x => new { x.OwnerId, x.ContentId });
                    table.ForeignKey(
                        name: "FK_SavedContents_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedContents_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoMetas",
                columns: table => new
                {
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoMetas", x => x.ContentId);
                    table.ForeignKey(
                        name: "FK_VideoMetas_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CommentatorId",
                table: "Comments",
                column: "CommentatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ContentId",
                table: "Comments",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PublicatedDate",
                table: "Comments",
                column: "PublicatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_CreatedDate",
                table: "Contents",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_CreatorId",
                table: "Contents",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_DomainId",
                table: "Contents",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_Slug",
                table: "Contents",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contents_Title",
                table: "Contents",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

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

            migrationBuilder.CreateIndex(
                name: "IX_LikedContents_ContentId",
                table: "LikedContents",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedContents_ContentId",
                table: "SavedContents",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Slug",
                table: "Users",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscribtions_UserId",
                table: "UserSubscribtions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "DomainOwners");

            migrationBuilder.DropTable(
                name: "DomainSubscriptions");

            migrationBuilder.DropTable(
                name: "LikedContents");

            migrationBuilder.DropTable(
                name: "SavedContents");

            migrationBuilder.DropTable(
                name: "UserSubscribtions");

            migrationBuilder.DropTable(
                name: "VideoMetas");

            migrationBuilder.DropTable(
                name: "Contents");

            migrationBuilder.DropTable(
                name: "Domains");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
