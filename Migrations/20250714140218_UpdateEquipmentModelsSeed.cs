using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEENALOoFINALE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEquipmentModelsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 1,
                column: "ModelName",
                value: "Black Dragon lenovo v4 projector");

            migrationBuilder.UpdateData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 2,
                columns: new[] { "EquipmentTypeId", "ModelName" },
                values: new object[] { 2, "21D model 6 Hisense air conditioner" });

            migrationBuilder.UpdateData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 3,
                column: "ModelName",
                value: "21D model 4 Hisense air conditioner");

            migrationBuilder.UpdateData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 4,
                columns: new[] { "EquipmentTypeId", "ModelName" },
                values: new object[] { 1, "2005 Metallic back grey projector" });

            migrationBuilder.UpdateData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 5,
                column: "ModelName",
                value: "XX5 Dragon Podium");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 1,
                column: "ModelName",
                value: "Projector Model A");

            migrationBuilder.UpdateData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 2,
                columns: new[] { "EquipmentTypeId", "ModelName" },
                values: new object[] { 1, "Projector Model B" });

            migrationBuilder.UpdateData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 3,
                column: "ModelName",
                value: "Air Conditioner Model A");

            migrationBuilder.UpdateData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 4,
                columns: new[] { "EquipmentTypeId", "ModelName" },
                values: new object[] { 2, "Air Conditioner Model B" });

            migrationBuilder.UpdateData(
                table: "EquipmentModels",
                keyColumn: "EquipmentModelId",
                keyValue: 5,
                column: "ModelName",
                value: "Podium Model A");

            migrationBuilder.InsertData(
                table: "EquipmentModels",
                columns: new[] { "EquipmentModelId", "EquipmentTypeId", "ModelName" },
                values: new object[] { 6, 3, "Podium Model B" });
        }
    }
}
