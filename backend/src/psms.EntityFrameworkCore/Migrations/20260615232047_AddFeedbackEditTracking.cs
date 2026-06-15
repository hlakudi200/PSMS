using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackEditTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeedbackHistory",
                table: "Marks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarksReleasedDate",
                table: "Assessments",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedbackHistory",
                table: "Marks");

            migrationBuilder.DropColumn(
                name: "MarksReleasedDate",
                table: "Assessments");
        }
    }
}
