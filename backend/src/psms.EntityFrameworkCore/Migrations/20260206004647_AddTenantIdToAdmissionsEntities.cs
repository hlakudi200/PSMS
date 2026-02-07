using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToAdmissionsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ApplicationFees",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ApplicationDocuments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ApplicantParents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AdmissionInterviews",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AdmissionAssessments",
                type: "integer",
                nullable: true);

            // Backfill TenantId from parent Application to prevent NULL = visible to all tenants
            migrationBuilder.Sql(@"UPDATE ""ApplicantParents"" SET ""TenantId"" = a.""TenantId"" FROM ""Applications"" a WHERE ""ApplicantParents"".""ApplicationId"" = a.""Id"";");
            migrationBuilder.Sql(@"UPDATE ""ApplicationFees"" SET ""TenantId"" = a.""TenantId"" FROM ""Applications"" a WHERE ""ApplicationFees"".""ApplicationId"" = a.""Id"";");
            migrationBuilder.Sql(@"UPDATE ""ApplicationDocuments"" SET ""TenantId"" = a.""TenantId"" FROM ""Applications"" a WHERE ""ApplicationDocuments"".""ApplicationId"" = a.""Id"";");
            migrationBuilder.Sql(@"UPDATE ""AdmissionInterviews"" SET ""TenantId"" = a.""TenantId"" FROM ""Applications"" a WHERE ""AdmissionInterviews"".""ApplicationId"" = a.""Id"";");
            migrationBuilder.Sql(@"UPDATE ""AdmissionAssessments"" SET ""TenantId"" = a.""TenantId"" FROM ""Applications"" a WHERE ""AdmissionAssessments"".""ApplicationId"" = a.""Id"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApplicationFees");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApplicantParents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AdmissionInterviews");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AdmissionAssessments");
        }
    }
}
