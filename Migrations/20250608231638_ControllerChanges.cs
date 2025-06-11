using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FEENALOoFINALE.Migrations
{
    /// <inheritdoc />
    public partial class ControllerChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ModelName",
                table: "EquipmentModels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.InsertData(
                table: "EquipmentModels",
                columns: new[] { "EquipmentModelId", "EquipmentTypeId", "ModelName" },
                values: new object[,]
                {
                    { 1, 1, "Projector Model A" },
                    { 2, 1, "Projector Model B" },
                    { 3, 2, "Air Conditioner Model A" },
                    { 4, 2, "Air Conditioner Model B" },
                    { 5, 3, "Podium Model A" },
                    { 6, 3, "Podium Model B" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 6);

            migrationBuilder.AlterColumn<string>(
                name: "ModelName",
                table: "EquipmentModels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
