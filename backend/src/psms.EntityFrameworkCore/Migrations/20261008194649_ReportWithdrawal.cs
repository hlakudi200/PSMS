using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class ReportWithdrawal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WithdrawalReason",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WithdrawnByUserId",
                table: "Reports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WithdrawnDate",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WithdrawalReason",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "WithdrawnByUserId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "WithdrawnDate",
                table: "Reports");
        }
    }
}
