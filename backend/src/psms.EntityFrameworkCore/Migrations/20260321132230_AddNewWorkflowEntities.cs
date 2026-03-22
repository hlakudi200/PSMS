using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AddNewWorkflowEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DisciplinaryCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    CaseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IncidentDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IncidentCategory = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WitnessNames = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReportedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportedByName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InvestigationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HearingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HearingNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Outcome = table.Column<int>(type: "integer", nullable: true),
                    OutcomeDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SanctionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SanctionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ParentNotified = table.Column<bool>(type: "boolean", nullable: false),
                    ParentNotifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppealNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_DisciplinaryCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisciplinaryCases_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DisciplinaryCases_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    RequestNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    RequestedByName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AcademicYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Vendor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    QuotationUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    InvoiceUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ReceiptUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    RequiredByDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PaymentReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ExpenseRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseRequests_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeWaivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentFeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcademicYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    WaiverType = table.Column<int>(type: "integer", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SupportingDocumentUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ReviewedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FeeWaivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeWaivers_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeWaivers_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldTrips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    TripName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AcademicYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    TermId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizingTeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    GradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Destination = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TripDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    NumberOfStudents = table.Column<int>(type: "integer", nullable: false),
                    NumberOfChaperones = table.Column<int>(type: "integer", nullable: false),
                    TransportArrangement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RiskAssessmentNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EmergencyPlan = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApprovedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FieldTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldTrips_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FieldTrips_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FieldTrips_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FieldTrips_Teachers_OrganizingTeacherId",
                        column: x => x.OrganizingTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FieldTrips_Terms_TermId",
                        column: x => x.TermId,
                        principalTable: "Terms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffLeaveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    LeaveNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LeaveType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SubstituteTeacherId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubstituteTeacherName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApprovedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SupportingDocumentUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
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
                    table.PrimaryKey("PK_StaffLeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffLeaveRequests_Teachers_SubstituteTeacherId",
                        column: x => x.SubstituteTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentTransferRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    TransferNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FromSchoolName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ToSchoolName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequestedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TransferGradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousReportUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    TransferCertificateUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ApprovedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_StudentTransferRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTransferRequests_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentTransferRequests_Grades_TransferGradeId",
                        column: x => x.TransferGradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentTransferRequests_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisciplinaryCases_AcademicYearId",
                table: "DisciplinaryCases",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_DisciplinaryCases_StudentId",
                table: "DisciplinaryCases",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_DisciplinaryCases_TenantId_CaseNumber",
                table: "DisciplinaryCases",
                columns: new[] { "TenantId", "CaseNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DisciplinaryCases_TenantId_StudentId_Status",
                table: "DisciplinaryCases",
                columns: new[] { "TenantId", "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseRequests_AcademicYearId",
                table: "ExpenseRequests",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseRequests_TenantId_RequestNumber",
                table: "ExpenseRequests",
                columns: new[] { "TenantId", "RequestNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FeeWaivers_AcademicYearId",
                table: "FeeWaivers",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeWaivers_StudentId",
                table: "FeeWaivers",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeWaivers_TenantId_StudentId_AcademicYearId",
                table: "FeeWaivers",
                columns: new[] { "TenantId", "StudentId", "AcademicYearId" });

            migrationBuilder.CreateIndex(
                name: "IX_FieldTrips_AcademicYearId",
                table: "FieldTrips",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldTrips_ClassId",
                table: "FieldTrips",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldTrips_GradeId",
                table: "FieldTrips",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldTrips_OrganizingTeacherId",
                table: "FieldTrips",
                column: "OrganizingTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldTrips_TenantId_AcademicYearId_Status",
                table: "FieldTrips",
                columns: new[] { "TenantId", "AcademicYearId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FieldTrips_TermId",
                table: "FieldTrips",
                column: "TermId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLeaveRequests_SubstituteTeacherId",
                table: "StaffLeaveRequests",
                column: "SubstituteTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLeaveRequests_TenantId_LeaveNumber",
                table: "StaffLeaveRequests",
                columns: new[] { "TenantId", "LeaveNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_StaffLeaveRequests_TenantId_UserId_Status",
                table: "StaffLeaveRequests",
                columns: new[] { "TenantId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransferRequests_AcademicYearId",
                table: "StudentTransferRequests",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransferRequests_StudentId",
                table: "StudentTransferRequests",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransferRequests_TenantId_TransferNumber",
                table: "StudentTransferRequests",
                columns: new[] { "TenantId", "TransferNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransferRequests_TransferGradeId",
                table: "StudentTransferRequests",
                column: "TransferGradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisciplinaryCases");

            migrationBuilder.DropTable(
                name: "ExpenseRequests");

            migrationBuilder.DropTable(
                name: "FeeWaivers");

            migrationBuilder.DropTable(
                name: "FieldTrips");

            migrationBuilder.DropTable(
                name: "StaffLeaveRequests");

            migrationBuilder.DropTable(
                name: "StudentTransferRequests");
        }
    }
}
