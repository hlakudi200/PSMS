using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AddReportVerificationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerificationToken",
                table: "Reports",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_VerificationToken",
                table: "Reports",
                column: "VerificationToken",
                unique: true,
                filter: "\"VerificationToken\" IS NOT NULL AND \"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_VerificationToken",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "VerificationToken",
                table: "Reports");
        }
    }
}
