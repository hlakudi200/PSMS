using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AddOnlineLessonMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnlineLessonMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    OnlineLessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    LearningMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineLessonMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineLessonMaterials_LearningMaterials_LearningMaterialId",
                        column: x => x.LearningMaterialId,
                        principalTable: "LearningMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OnlineLessonMaterials_OnlineLessons_OnlineLessonId",
                        column: x => x.OnlineLessonId,
                        principalTable: "OnlineLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnlineLessonMaterials_LearningMaterialId",
                table: "OnlineLessonMaterials",
                column: "LearningMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineLessonMaterials_OnlineLessonId_LearningMaterialId",
                table: "OnlineLessonMaterials",
                columns: new[] { "OnlineLessonId", "LearningMaterialId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnlineLessonMaterials");
        }
    }
}
