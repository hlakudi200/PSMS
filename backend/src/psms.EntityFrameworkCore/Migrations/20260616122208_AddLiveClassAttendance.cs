using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveClassAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveClassAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    OnlineLessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantIdentity = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FirstJoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveClassAttendances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveClassAttendances_OnlineLessonId_ParticipantIdentity",
                table: "LiveClassAttendances",
                columns: new[] { "OnlineLessonId", "ParticipantIdentity" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveClassAttendances");
        }
    }
}
