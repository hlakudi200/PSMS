using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class UniqueIndexesIgnoreSoftDeletedRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_DefinitionId_StepOrder",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_TenantId_EntityType_EntityId_Active",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_Waitlists_ApplicationId",
                table: "Waitlists");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_TenantId_EmployeeNumber",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_StudentTransports_StudentId_TransportId_YearId",
                table: "StudentTransports");

            migrationBuilder.DropIndex(
                name: "IX_Students_TenantId_AdmissionNumber",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_StudentFees_StudentId_FeeStructureId",
                table: "StudentFees");

            migrationBuilder.DropIndex(
                name: "IX_StudentExtramurals_StudentId_ActivityId_YearId",
                table: "StudentExtramurals");

            migrationBuilder.DropIndex(
                name: "IX_StudentClasses_StudentId_ClassId_AcademicYearId",
                table: "StudentClasses");

            migrationBuilder.DropIndex(
                name: "IX_StudentAfterCares_StudentId_AfterCareId_YearId",
                table: "StudentAfterCares");

            migrationBuilder.DropIndex(
                name: "IX_Reports_StudentId_TermId_ReportType",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Marks_AssessmentId_StudentId",
                table: "Marks");

            migrationBuilder.DropIndex(
                name: "IX_LearningMaterialVersions_MaterialId_VersionNumber",
                table: "LearningMaterialVersions");

            migrationBuilder.DropIndex(
                name: "IX_ClassSubjects_ClassId_SubjectId",
                table: "ClassSubjects");

            migrationBuilder.DropIndex(
                name: "IX_AdmissionSettings_AcademicYearId_GradeId",
                table: "AdmissionSettings");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_DefinitionId_StepOrder",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowDefinitionId", "StepOrder" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TenantId_EntityType_EntityId_Active",
                table: "WorkflowInstances",
                columns: new[] { "TenantId", "EntityType", "EntityId" },
                unique: true,
                filter: "\"Status\" IN (1, 2) AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Waitlists_ApplicationId",
                table: "Waitlists",
                column: "ApplicationId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_TenantId_EmployeeNumber",
                table: "Teachers",
                columns: new[] { "TenantId", "EmployeeNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransports_StudentId_TransportId_YearId",
                table: "StudentTransports",
                columns: new[] { "StudentId", "SchoolTransportId", "AcademicYearId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Students_TenantId_AdmissionNumber",
                table: "Students",
                columns: new[] { "TenantId", "AdmissionNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_StudentId_FeeStructureId",
                table: "StudentFees",
                columns: new[] { "StudentId", "FeeStructureId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExtramurals_StudentId_ActivityId_YearId",
                table: "StudentExtramurals",
                columns: new[] { "StudentId", "ExtramuralActivityId", "AcademicYearId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_StudentId_ClassId_AcademicYearId",
                table: "StudentClasses",
                columns: new[] { "StudentId", "ClassId", "AcademicYearId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAfterCares_StudentId_AfterCareId_YearId",
                table: "StudentAfterCares",
                columns: new[] { "StudentId", "AfterCareId", "AcademicYearId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StudentId_TermId_ReportType",
                table: "Reports",
                columns: new[] { "StudentId", "TermId", "ReportType" },
                unique: true,
                filter: "\"TermId\" IS NOT NULL AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Marks_AssessmentId_StudentId",
                table: "Marks",
                columns: new[] { "AssessmentId", "StudentId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LearningMaterialVersions_MaterialId_VersionNumber",
                table: "LearningMaterialVersions",
                columns: new[] { "LearningMaterialId", "VersionNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_ClassId_SubjectId",
                table: "ClassSubjects",
                columns: new[] { "ClassId", "SubjectId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionSettings_AcademicYearId_GradeId",
                table: "AdmissionSettings",
                columns: new[] { "AcademicYearId", "GradeId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_DefinitionId_StepOrder",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_TenantId_EntityType_EntityId_Active",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_Waitlists_ApplicationId",
                table: "Waitlists");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_TenantId_EmployeeNumber",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_StudentTransports_StudentId_TransportId_YearId",
                table: "StudentTransports");

            migrationBuilder.DropIndex(
                name: "IX_Students_TenantId_AdmissionNumber",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_StudentFees_StudentId_FeeStructureId",
                table: "StudentFees");

            migrationBuilder.DropIndex(
                name: "IX_StudentExtramurals_StudentId_ActivityId_YearId",
                table: "StudentExtramurals");

            migrationBuilder.DropIndex(
                name: "IX_StudentClasses_StudentId_ClassId_AcademicYearId",
                table: "StudentClasses");

            migrationBuilder.DropIndex(
                name: "IX_StudentAfterCares_StudentId_AfterCareId_YearId",
                table: "StudentAfterCares");

            migrationBuilder.DropIndex(
                name: "IX_Reports_StudentId_TermId_ReportType",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Marks_AssessmentId_StudentId",
                table: "Marks");

            migrationBuilder.DropIndex(
                name: "IX_LearningMaterialVersions_MaterialId_VersionNumber",
                table: "LearningMaterialVersions");

            migrationBuilder.DropIndex(
                name: "IX_ClassSubjects_ClassId_SubjectId",
                table: "ClassSubjects");

            migrationBuilder.DropIndex(
                name: "IX_AdmissionSettings_AcademicYearId_GradeId",
                table: "AdmissionSettings");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_DefinitionId_StepOrder",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowDefinitionId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TenantId_EntityType_EntityId_Active",
                table: "WorkflowInstances",
                columns: new[] { "TenantId", "EntityType", "EntityId" },
                unique: true,
                filter: "\"Status\" IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_Waitlists_ApplicationId",
                table: "Waitlists",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_TenantId_EmployeeNumber",
                table: "Teachers",
                columns: new[] { "TenantId", "EmployeeNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransports_StudentId_TransportId_YearId",
                table: "StudentTransports",
                columns: new[] { "StudentId", "SchoolTransportId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_TenantId_AdmissionNumber",
                table: "Students",
                columns: new[] { "TenantId", "AdmissionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_StudentId_FeeStructureId",
                table: "StudentFees",
                columns: new[] { "StudentId", "FeeStructureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentExtramurals_StudentId_ActivityId_YearId",
                table: "StudentExtramurals",
                columns: new[] { "StudentId", "ExtramuralActivityId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_StudentId_ClassId_AcademicYearId",
                table: "StudentClasses",
                columns: new[] { "StudentId", "ClassId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentAfterCares_StudentId_AfterCareId_YearId",
                table: "StudentAfterCares",
                columns: new[] { "StudentId", "AfterCareId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StudentId_TermId_ReportType",
                table: "Reports",
                columns: new[] { "StudentId", "TermId", "ReportType" },
                unique: true,
                filter: "\"TermId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Marks_AssessmentId_StudentId",
                table: "Marks",
                columns: new[] { "AssessmentId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningMaterialVersions_MaterialId_VersionNumber",
                table: "LearningMaterialVersions",
                columns: new[] { "LearningMaterialId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_ClassId_SubjectId",
                table: "ClassSubjects",
                columns: new[] { "ClassId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionSettings_AcademicYearId_GradeId",
                table: "AdmissionSettings",
                columns: new[] { "AcademicYearId", "GradeId" },
                unique: true);
        }
    }
}
