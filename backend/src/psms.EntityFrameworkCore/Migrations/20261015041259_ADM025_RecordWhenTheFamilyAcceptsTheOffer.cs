using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class ADM025_RecordWhenTheFamilyAcceptsTheOffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OfferAcceptedByUserId",
                table: "Applications",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferAcceptedDate",
                table: "Applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferDeclineReason",
                table: "Applications",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferDeclinedDate",
                table: "Applications",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfferAcceptedByUserId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OfferAcceptedDate",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OfferDeclineReason",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OfferDeclinedDate",
                table: "Applications");
        }
    }
}
