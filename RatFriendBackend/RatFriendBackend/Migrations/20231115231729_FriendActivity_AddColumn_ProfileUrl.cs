using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RatFriendBackend.Migrations
{
    /// <inheritdoc />
    public partial class FriendActivity_AddColumn_ProfileUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileUrl",
                table: "FriendActivities",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileUrl",
                table: "FriendActivities");
        }
    }
}
