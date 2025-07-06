using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEENALOoFINALE.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedWorkflowImplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "MaintenanceTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedFromAlertId",
                table: "MaintenanceTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginatingAlertAlertId",
                table: "MaintenanceTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "MaintenanceTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "MaintenanceLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_OriginatingAlertAlertId",
                table: "MaintenanceTasks",
                column: "OriginatingAlertAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLogs_TaskId",
                table: "MaintenanceLogs",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceLogs_MaintenanceTasks_TaskId",
                table: "MaintenanceLogs",
                column: "TaskId",
                principalTable: "MaintenanceTasks",
                principalColumn: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceTasks_Alerts_OriginatingAlertAlertId",
                table: "MaintenanceTasks",
                column: "OriginatingAlertAlertId",
                principalTable: "Alerts",
                principalColumn: "AlertId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceLogs_MaintenanceTasks_TaskId",
                table: "MaintenanceLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceTasks_Alerts_OriginatingAlertAlertId",
                table: "MaintenanceTasks");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceTasks_OriginatingAlertAlertId",
                table: "MaintenanceTasks");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceLogs_TaskId",
                table: "MaintenanceLogs");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "CreatedFromAlertId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "OriginatingAlertAlertId",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "MaintenanceLogs");
        }
    }
}
