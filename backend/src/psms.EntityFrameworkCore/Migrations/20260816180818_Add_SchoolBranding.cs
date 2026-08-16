using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class Add_SchoolBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolBrandings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    PrimaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SecondaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoObjectKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FaviconUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FaviconObjectKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SchoolName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolBrandings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBrandings_TenantId",
                table: "SchoolBrandings",
                column: "TenantId",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolBrandings");
        }
    }
}
