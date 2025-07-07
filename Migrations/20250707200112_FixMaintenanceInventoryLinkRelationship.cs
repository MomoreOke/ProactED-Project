using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEENALOoFINALE.Migrations
{
    /// <inheritdoc />
    public partial class FixMaintenanceInventoryLinkRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceInventoryLinks_InventoryItems_ItemId",
                table: "MaintenanceInventoryLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceInventoryLinks_MaintenanceLogs_MaintenanceLogLogId",
                table: "MaintenanceInventoryLinks");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceInventoryLinks_MaintenanceLogLogId",
                table: "MaintenanceInventoryLinks");

            migrationBuilder.DropColumn(
                name: "MaintenanceLogLogId",
                table: "MaintenanceInventoryLinks");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceInventoryLinks_LogId",
                table: "MaintenanceInventoryLinks",
                column: "LogId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceInventoryLinks_InventoryItems_ItemId",
                table: "MaintenanceInventoryLinks",
                column: "ItemId",
                principalTable: "InventoryItems",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceInventoryLinks_MaintenanceLogs_LogId",
                table: "MaintenanceInventoryLinks",
                column: "LogId",
                principalTable: "MaintenanceLogs",
                principalColumn: "LogId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceInventoryLinks_InventoryItems_ItemId",
                table: "MaintenanceInventoryLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceInventoryLinks_MaintenanceLogs_LogId",
                table: "MaintenanceInventoryLinks");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceInventoryLinks_LogId",
                table: "MaintenanceInventoryLinks");

            migrationBuilder.AddColumn<int>(
                name: "MaintenanceLogLogId",
                table: "MaintenanceInventoryLinks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceInventoryLinks_MaintenanceLogLogId",
                table: "MaintenanceInventoryLinks",
                column: "MaintenanceLogLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceInventoryLinks_InventoryItems_ItemId",
                table: "MaintenanceInventoryLinks",
                column: "ItemId",
                principalTable: "InventoryItems",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceInventoryLinks_MaintenanceLogs_MaintenanceLogLogId",
                table: "MaintenanceInventoryLinks",
                column: "MaintenanceLogLogId",
                principalTable: "MaintenanceLogs",
                principalColumn: "LogId");
        }
    }
}
