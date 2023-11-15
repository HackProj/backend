using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using RatFriendBackend.Persistence.Models;

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
                name: "FriendActivityInfos",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    FriendId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LastPlayTime = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    StillPlayingWithoutUs = table.Column<bool>(type: "boolean", nullable: false),
                    GameInfos = table.Column<List<GamesInfo>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendActivityInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ApiToken = table.Column<string>(type: "text", nullable: false),
                    LastPlayTime = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserFriendActivities",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    FriendId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    FriendActivityInfoId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFriendActivities", x => new { x.UserId, x.FriendId });
                    table.ForeignKey(
                        name: "FK_UserFriendActivities_FriendActivityInfos_FriendActivityInfo~",
                        column: x => x.FriendActivityInfoId,
                        principalTable: "FriendActivityInfos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserFriendActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFriendActivities_FriendActivityInfoId",
                table: "UserFriendActivities",
                column: "FriendActivityInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFriendActivities");

            migrationBuilder.DropTable(
                name: "FriendActivityInfos");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
