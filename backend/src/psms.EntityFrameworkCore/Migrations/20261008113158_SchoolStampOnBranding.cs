using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class SchoolStampOnBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StampObjectKey",
                table: "SchoolBrandings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StampUrl",
                table: "SchoolBrandings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StampObjectKey",
                table: "SchoolBrandings");

            migrationBuilder.DropColumn(
                name: "StampUrl",
                table: "SchoolBrandings");
        }
    }
}
