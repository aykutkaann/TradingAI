using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForSubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlanId",
                table: "user_subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_PlanId",
                table: "user_subscriptions",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_subscriptions_subscription_plans_PlanId",
                table: "user_subscriptions",
                column: "PlanId",
                principalTable: "subscription_plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_subscriptions_subscription_plans_PlanId",
                table: "user_subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_user_subscriptions_PlanId",
                table: "user_subscriptions");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "user_subscriptions");
        }
    }
}
