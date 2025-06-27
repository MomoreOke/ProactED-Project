using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEENALOoFINALE.Migrations
{
    /// <inheritdoc />
    public partial class FinishingTouches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "InventoryItems");

            migrationBuilder.AddColumn<int>(
                name: "AlertId",
                table: "MaintenanceLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLogs_AlertId",
                table: "MaintenanceLogs",
                column: "AlertId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceLogs_Alerts_AlertId",
                table: "MaintenanceLogs",
                column: "AlertId",
                principalTable: "Alerts",
                principalColumn: "AlertId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceLogs_Alerts_AlertId",
                table: "MaintenanceLogs");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceLogs_AlertId",
                table: "MaintenanceLogs");

            migrationBuilder.DropColumn(
                name: "AlertId",
                table: "MaintenanceLogs");

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
