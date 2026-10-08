using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class RetentionProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RetentionProcedures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffMeetingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StaffMeetingNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ParentMeetingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ParentMeetingNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ParentConfirmedInWritingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ParentConfirmationReference = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AppealLodgedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppealDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppealDeterminationDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppealDeterminedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppealOutcome = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_RetentionProcedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetentionProcedures_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RetentionProcedures_ReportId",
                table: "RetentionProcedures",
                column: "ReportId",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RetentionProcedures");
        }
    }
}
