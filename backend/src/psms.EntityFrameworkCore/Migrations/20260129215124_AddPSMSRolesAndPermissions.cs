using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AddPSMSRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "Applications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "AdmissionSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    AcademicYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicationFeeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: true),
                    CurrentEnrolledCount = table.Column<int>(type: "integer", nullable: false),
                    ApplicationOpenDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApplicationCloseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsAcceptingApplications = table.Column<bool>(type: "boolean", nullable: false),
                    RequiredDocuments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsInterviewRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsAssessmentRequired = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumAge = table.Column<int>(type: "integer", nullable: true),
                    MaximumAge = table.Column<int>(type: "integer", nullable: true),
                    OfferExpiryDays = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdmissionSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdmissionSettings_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdmissionSettings_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_AcademicYearId",
                table: "Applications",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionSettings_AcademicYearId_GradeId",
                table: "AdmissionSettings",
                columns: new[] { "AcademicYearId", "GradeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionSettings_GradeId",
                table: "AdmissionSettings",
                column: "GradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_AcademicYears_AcademicYearId",
                table: "Applications",
                column: "AcademicYearId",
                principalTable: "AcademicYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_AcademicYears_AcademicYearId",
                table: "Applications");

            migrationBuilder.DropTable(
                name: "AdmissionSettings");

            migrationBuilder.DropIndex(
                name: "IX_Applications_AcademicYearId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Applications");
        }
    }
}
