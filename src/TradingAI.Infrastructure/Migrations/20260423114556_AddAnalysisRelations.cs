using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_analyses_users_UserId",
                table: "analyses");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "analyses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiAnalysis",
                table: "analyses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AssetId",
                table: "analyses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateOnly>(
                name: "CreatedAt",
                table: "analyses",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "DetectedPatterns",
                table: "analyses",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "analyses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "analyses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KeyLevels",
                table: "analyses",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Outcome",
                table: "analyses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OutcomeCheckedAt",
                table: "analyses",
                type: "timestamp with time zone",
                nullable: true);

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

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAtAnalysis",
                table: "analyses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RiskRewardRatio",
                table: "analyses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StopLoss",
                table: "analyses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SuggestedEntry",
                table: "analyses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "analyses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TakeProfit1",
                table: "analyses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TakeProfit2",
                table: "analyses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeFrame",
                table: "analyses",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrendDirection",
                table: "analyses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "analyses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserPrompt",
                table: "analyses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "analysis_comment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AnalysisId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_analysis_comment_analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_analysis_comment_analyses_AnalysisId1",
                        column: x => x.AnalysisId1,
                        principalTable: "analyses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_analysis_comment_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "analysis_like",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AnalysisId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_like", x => x.Id);
                    table.ForeignKey(
                        name: "FK_analysis_like_analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_analysis_like_analyses_AnalysisId1",
                        column: x => x.AnalysisId1,
                        principalTable: "analyses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_analysis_like_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analyses_AssetId",
                table: "analyses",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_analyses_CreatedAt",
                table: "analyses",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_analyses_IsPublished",
                table: "analyses",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_comment_AnalysisId",
                table: "analysis_comment",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_comment_AnalysisId1",
                table: "analysis_comment",
                column: "AnalysisId1");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_comment_UserId",
                table: "analysis_comment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_like_AnalysisId_UserId",
                table: "analysis_like",
                columns: new[] { "AnalysisId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_analysis_like_AnalysisId1",
                table: "analysis_like",
                column: "AnalysisId1");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_like_UserId",
                table: "analysis_like",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_analyses_assets_AssetId",
                table: "analyses",
                column: "AssetId",
                principalTable: "assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_analyses_users_UserId",
                table: "analyses",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_analyses_assets_AssetId",
                table: "analyses");

            migrationBuilder.DropForeignKey(
                name: "FK_analyses_users_UserId",
                table: "analyses");

            migrationBuilder.DropTable(
                name: "analysis_comment");

            migrationBuilder.DropTable(
                name: "analysis_like");

            migrationBuilder.DropIndex(
                name: "IX_analyses_AssetId",
                table: "analyses");

            migrationBuilder.DropIndex(
                name: "IX_analyses_CreatedAt",
                table: "analyses");

            migrationBuilder.DropIndex(
                name: "IX_analyses_IsPublished",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "AiAnalysis",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "DetectedPatterns",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "KeyLevels",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "OutcomeCheckedAt",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "PriceAfter24h",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "PriceAfter7d",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "PriceAtAnalysis",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "RiskRewardRatio",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "StopLoss",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "SuggestedEntry",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "TakeProfit1",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "TakeProfit2",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "TimeFrame",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "TrendDirection",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "analyses");

            migrationBuilder.DropColumn(
                name: "UserPrompt",
                table: "analyses");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "analyses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_analyses_users_UserId",
                table: "analyses",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}
