using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEENALOoFINALE.Migrations
{
    /// <inheritdoc />
    public partial class RenameTaskIdToMaintenanceTaskId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceLogs_MaintenanceTasks_TaskId",
                table: "MaintenanceLogs");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "MaintenanceLogs",
                newName: "MaintenanceTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceLogs_TaskId",
                table: "MaintenanceLogs",
                newName: "IX_MaintenanceLogs_MaintenanceTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceLogs_MaintenanceTasks_MaintenanceTaskId",
                table: "MaintenanceLogs",
                column: "MaintenanceTaskId",
                principalTable: "MaintenanceTasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceLogs_MaintenanceTasks_MaintenanceTaskId",
                table: "MaintenanceLogs");

            migrationBuilder.RenameColumn(
                name: "MaintenanceTaskId",
                table: "MaintenanceLogs",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceLogs_MaintenanceTaskId",
                table: "MaintenanceLogs",
                newName: "IX_MaintenanceLogs_TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceLogs_MaintenanceTasks_TaskId",
                table: "MaintenanceLogs",
                column: "TaskId",
                principalTable: "MaintenanceTasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
