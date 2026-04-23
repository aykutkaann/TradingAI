using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublised : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "analyses",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "analyses");
        }
    }
}
