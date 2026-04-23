using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserFollowConfigUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_follows_users_FollowingId",
                table: "user_follows");

            migrationBuilder.AddForeignKey(
                name: "FK_user_follows_users_FollowingId",
                table: "user_follows",
                column: "FollowingId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_follows_users_FollowingId",
                table: "user_follows");

            migrationBuilder.AddForeignKey(
                name: "FK_user_follows_users_FollowingId",
                table: "user_follows",
                column: "FollowingId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
