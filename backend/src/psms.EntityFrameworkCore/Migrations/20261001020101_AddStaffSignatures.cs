using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffSignatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrincipalSignatureSvg",
                table: "Reports",
                type: "character varying(65536)",
                maxLength: 65536,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherSignatureSvg",
                table: "Reports",
                type: "character varying(65536)",
                maxLength: 65536,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StaffSignatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    SvgContent = table.Column<string>(type: "character varying(65536)", maxLength: 65536, nullable: false),
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
                    table.PrimaryKey("PK_StaffSignatures", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffSignatures_TenantId_UserId",
                table: "StaffSignatures",
                columns: new[] { "TenantId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffSignatures");

            migrationBuilder.DropColumn(
                name: "PrincipalSignatureSvg",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TeacherSignatureSvg",
                table: "Reports");
        }
    }
}
