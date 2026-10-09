using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationFeeIsOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // True, not false: every school already configured here charges what
            // it charges, and a column that defaulted to "no fee" would quietly
            // stop them collecting it.
            migrationBuilder.AddColumn<bool>(
                name: "IsApplicationFeeRequired",
                table: "AdmissionSettings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // And a row that was already charging nothing was already not
            // charging. Say so, rather than leaving it looking like a fee that
            // happens to be zero.
            migrationBuilder.Sql(
                @"UPDATE ""AdmissionSettings"" SET ""IsApplicationFeeRequired"" = false
                  WHERE ""ApplicationFeeAmount"" <= 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApplicationFeeRequired",
                table: "AdmissionSettings");
        }
    }
}
