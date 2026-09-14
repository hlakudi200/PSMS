using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class WF30_WorkflowGuardsEffectsDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuardExpression",
                table: "WorkflowSteps");

            migrationBuilder.AddColumn<string>(
                name: "DecisionJson",
                table: "WorkflowTransitions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGuardOverridden",
                table: "WorkflowTransitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWaived",
                table: "WorkflowTransitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DecisionSchemaKey",
                table: "WorkflowSteps",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntryEffectKey",
                table: "WorkflowSteps",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExitEffectKey",
                table: "WorkflowSteps",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuardKey",
                table: "WorkflowSteps",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "WorkflowSteps",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecisionJson",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "IsGuardOverridden",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "IsWaived",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "DecisionSchemaKey",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "EntryEffectKey",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "ExitEffectKey",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "GuardKey",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "WorkflowSteps");

            migrationBuilder.AddColumn<string>(
                name: "GuardExpression",
                table: "WorkflowSteps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
