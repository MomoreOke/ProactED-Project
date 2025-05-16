using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEENALOoFINALE.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentQuantity",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "FailurePredictions");

            migrationBuilder.DropColumn(
                name: "PredictedFailureType",
                table: "FailurePredictions");

            migrationBuilder.DropColumn(
                name: "PredictedFailureWindowEnd",
                table: "FailurePredictions");

            migrationBuilder.DropColumn(
                name: "AlertType",
                table: "Alerts");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "Department");

            migrationBuilder.RenameColumn(
                name: "LastRestockDate",
                table: "InventoryStocks",
                newName: "DateReceived");

            migrationBuilder.RenameColumn(
                name: "PredictionDate",
                table: "FailurePredictions",
                newName: "PredictedFailureDate");

            migrationBuilder.RenameColumn(
                name: "PredictedFailureWindowStart",
                table: "FailurePredictions",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "AlertDate",
                table: "Alerts",
                newName: "CreatedDate");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "DowntimeDuration",
                table: "MaintenanceLogs",
                type: "time",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MaintenanceLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "MaintenanceLogs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "MaintenanceDate",
                table: "MaintenanceLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MaintenanceLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "InventoryStocks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "InventoryStocks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "InventoryStocks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "CompatibleModels",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxStockLevel",
                table: "InventoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinimumStockLevel",
                table: "InventoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReorderPoint",
                table: "InventoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReorderQuantity",
                table: "InventoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisNotes",
                table: "FailurePredictions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfidenceLevel",
                table: "FailurePredictions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContributingFactors",
                table: "FailurePredictions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Equipment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedToUserId",
                table: "Alerts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MaintenanceTasks",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipmentId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceTasks", x => x.TaskId);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_AssignedToUserId",
                table: "MaintenanceTasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_EquipmentId",
                table: "MaintenanceTasks",
                column: "EquipmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "MaintenanceLogs");

            migrationBuilder.DropColumn(
                name: "MaintenanceDate",
                table: "MaintenanceLogs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MaintenanceLogs");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "MaxStockLevel",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "MinimumStockLevel",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ReorderPoint",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ReorderQuantity",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "AnalysisNotes",
                table: "FailurePredictions");

            migrationBuilder.DropColumn(
                name: "ConfidenceLevel",
                table: "FailurePredictions");

            migrationBuilder.DropColumn(
                name: "ContributingFactors",
                table: "FailurePredictions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Alerts");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "DateReceived",
                table: "InventoryStocks",
                newName: "LastRestockDate");

            migrationBuilder.RenameColumn(
                name: "PredictedFailureDate",
                table: "FailurePredictions",
                newName: "PredictionDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "FailurePredictions",
                newName: "PredictedFailureWindowStart");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Alerts",
                newName: "AlertDate");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "DowntimeDuration",
                table: "MaintenanceLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MaintenanceLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentQuantity",
                table: "InventoryStocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "InventoryStocks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CompatibleModels",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConfidenceScore",
                table: "FailurePredictions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PredictedFailureType",
                table: "FailurePredictions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PredictedFailureWindowEnd",
                table: "FailurePredictions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Equipment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AssignedToUserId",
                table: "Alerts",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AlertType",
                table: "Alerts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
