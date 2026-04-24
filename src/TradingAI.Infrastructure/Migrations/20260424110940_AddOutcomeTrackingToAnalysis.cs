using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutcomeTrackingToAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceAfter24h",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "PriceAfter7d",
                table: "analyses");

            migrationBuilder.AlterColumn<string>(
                name: "Pair",
                table: "analyses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Outcome",
                table: "analyses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "analyses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ResolvedPrice",
                table: "analyses",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StopLossHit",
                table: "analyses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TakeProfit1Hit",
                table: "analyses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TakeProfit2Hit",
                table: "analyses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_analyses_Outcome_ExpiresAt",
                table: "analyses",
                columns: new[] { "Outcome", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_analyses_Outcome_OutcomeCheckedAt",
                table: "analyses",
                columns: new[] { "Outcome", "OutcomeCheckedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_analyses_Outcome_ExpiresAt",
                table: "analyses");

            migrationBuilder.DropIndex(
                name: "IX_analyses_Outcome_OutcomeCheckedAt",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "ResolvedPrice",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "StopLossHit",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "TakeProfit1Hit",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "TakeProfit2Hit",
                table: "analyses");

            migrationBuilder.AlterColumn<string>(
                name: "Pair",
                table: "analyses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "Outcome",
                table: "analyses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAfter24h",
                table: "analyses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAfter7d",
                table: "analyses",
                type: "numeric",
                nullable: true);
        }
    }
}
