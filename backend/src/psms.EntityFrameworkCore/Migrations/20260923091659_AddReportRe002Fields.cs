using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AddReportRe002Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BehaviourComments",
                table: "Reports",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConductRating",
                table: "Reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysInTerm",
                table: "Reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiligenceRating",
                table: "Reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PrincipalSignedByUserId",
                table: "Reports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrincipalSignedDate",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportCardNumber",
                table: "Reports",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TeacherSignedByUserId",
                table: "Reports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TeacherSignedDate",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BehaviourComments",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ConductRating",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DaysInTerm",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DiligenceRating",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PrincipalSignedByUserId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PrincipalSignedDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportCardNumber",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TeacherSignedByUserId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TeacherSignedDate",
                table: "Reports");
        }
    }
}
