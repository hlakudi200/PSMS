using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class WF35_WorkflowStepOrderSoftDeleteFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_DefinitionId_StepOrder",
                table: "WorkflowSteps");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_DefinitionId_StepOrder",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowDefinitionId", "StepOrder" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_DefinitionId_StepOrder",
                table: "WorkflowSteps");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_DefinitionId_StepOrder",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowDefinitionId", "StepOrder" },
                unique: true);
        }
    }
}
