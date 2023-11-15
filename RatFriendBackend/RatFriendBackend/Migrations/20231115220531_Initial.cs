using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RatFriendBackend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FriendActivities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    FriendName = table.Column<string>(type: "text", nullable: false),
                    AppId = table.Column<long>(type: "bigint", nullable: false),
                    TimePlayed = table.Column<long>(type: "bigint", nullable: false),
                    AppName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscription",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    FriendId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IsFollowing = table.Column<bool>(type: "boolean", nullable: false),
                    WantHint = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscription", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FriendActivitySubscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserSubscriptionId = table.Column<long>(type: "bigint", nullable: false),
                    FriendActivityId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendActivitySubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FriendActivitySubscriptions_FriendActivities_FriendActivity~",
                        column: x => x.FriendActivityId,
                        principalTable: "FriendActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FriendActivitySubscriptions_UserSubscription_UserSubscripti~",
                        column: x => x.UserSubscriptionId,
                        principalTable: "UserSubscription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendActivities_FriendId_AppId",
                table: "FriendActivities",
                columns: new[] { "FriendId", "AppId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendActivitySubscriptions_FriendActivityId",
                table: "FriendActivitySubscriptions",
                column: "FriendActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendActivitySubscriptions_UserSubscriptionId",
                table: "FriendActivitySubscriptions",
                column: "UserSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_UserId_FriendId",
                table: "UserSubscription",
                columns: new[] { "UserId", "FriendId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendActivitySubscriptions");

            migrationBuilder.DropTable(
                name: "FriendActivities");

            migrationBuilder.DropTable(
                name: "UserSubscription");
        }
    }
}
