using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AdmissionsReviewEntityChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherSubjects_TeacherId",
                table: "TeacherSubjects");

            migrationBuilder.DropIndex(
                name: "IX_TeacherClasses_TeacherId",
                table: "TeacherClasses");

            migrationBuilder.DropIndex(
                name: "IX_StudentSubjects_StudentId",
                table: "StudentSubjects");

            migrationBuilder.DropIndex(
                name: "IX_GradeSubjects_GradeId",
                table: "GradeSubjects");

            migrationBuilder.DropIndex(
                name: "IX_Classes_GradeId_ClassName",
                table: "Classes");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ApplicationDocuments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RescheduleCount",
                table: "AdmissionInterviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_TeacherId_SubjectId_GradeId",
                table: "TeacherSubjects",
                columns: new[] { "TeacherId", "SubjectId", "GradeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClasses_TeacherId_ClassId_SubjectId",
                table: "TeacherClasses",
                columns: new[] { "TeacherId", "ClassId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_StudentId_SubjectId_AcademicYearId",
                table: "StudentSubjects",
                columns: new[] { "StudentId", "SubjectId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradeSubjects_GradeId_SubjectId",
                table: "GradeSubjects",
                columns: new[] { "GradeId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_GradeId_AcademicYearId_ClassName",
                table: "Classes",
                columns: new[] { "GradeId", "AcademicYearId", "ClassName" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherSubjects_TeacherId_SubjectId_GradeId",
                table: "TeacherSubjects");

            migrationBuilder.DropIndex(
                name: "IX_TeacherClasses_TeacherId_ClassId_SubjectId",
                table: "TeacherClasses");

            migrationBuilder.DropIndex(
                name: "IX_StudentSubjects_StudentId_SubjectId_AcademicYearId",
                table: "StudentSubjects");

            migrationBuilder.DropIndex(
                name: "IX_GradeSubjects_GradeId_SubjectId",
                table: "GradeSubjects");

            migrationBuilder.DropIndex(
                name: "IX_Classes_GradeId_AcademicYearId_ClassName",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ApplicationDocuments");

            migrationBuilder.DropColumn(
                name: "RescheduleCount",
                table: "AdmissionInterviews");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_TeacherId",
                table: "TeacherSubjects",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClasses_TeacherId",
                table: "TeacherClasses",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_StudentId",
                table: "StudentSubjects",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeSubjects_GradeId",
                table: "GradeSubjects",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_GradeId_ClassName",
                table: "Classes",
                columns: new[] { "GradeId", "ClassName" },
                unique: true);
        }
    }
}
