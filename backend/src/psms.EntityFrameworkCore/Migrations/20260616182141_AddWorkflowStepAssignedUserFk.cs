using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowStepAssignedUserFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_AssignedUserId",
                table: "WorkflowSteps",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_AbpUsers_AssignedUserId",
                table: "WorkflowSteps",
                column: "AssignedUserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_AbpUsers_AssignedUserId",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_AssignedUserId",
                table: "WorkflowSteps");
        }
    }
}
