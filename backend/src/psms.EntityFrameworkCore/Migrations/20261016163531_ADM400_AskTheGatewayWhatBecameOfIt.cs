using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <inheritdoc />
    public partial class ADM400_AskTheGatewayWhatBecameOfIt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatewayCheckoutId",
                table: "ApplicationFees",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GatewayCheckoutId",
                table: "ApplicationFees");
        }
    }
}
