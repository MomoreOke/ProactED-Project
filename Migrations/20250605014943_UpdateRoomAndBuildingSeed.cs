using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FEENALOoFINALE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoomAndBuildingSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RoomName",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.InsertData(
                table: "Buildings",
                columns: new[] { "BuildingId", "BuildingName" },
                values: new object[,]
                {
                    { 1, "Petroleum Building" },
                    { 2, "New Engineering Building" }
                });

            migrationBuilder.InsertData(
                table: "EquipmentTypes",
                columns: new[] { "EquipmentTypeId", "EquipmentTypeName" },
                values: new object[,]
                {
                    { 1, "Projectors" },
                    { 2, "Air Conditioners" },
                    { 3, "Podiums" }
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "BuildingId", "RoomName" },
                values: new object[,]
                {
                    { 1, 1, "PB001" },
                    { 2, 1, "PB012" },
                    { 3, 1, "PB014" },
                    { 4, 1, "PB020" },
                    { 5, 1, "PB201" },
                    { 6, 1, "PB208" },
                    { 7, 1, "PB214" },
                    { 8, 2, "NEB-GF" },
                    { 9, 2, "NEB-FF1" },
                    { 10, 2, "NEB-FF2" },
                    { 11, 2, "NEB-SF" },
                    { 12, 2, "NEB-TF" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EquipmentTypes",
                keyColumn: "EquipmentTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EquipmentTypes",
                keyColumn: "EquipmentTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EquipmentTypes",
                keyColumn: "EquipmentTypeId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Buildings",
                keyColumn: "BuildingId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Buildings",
                keyColumn: "BuildingId",
                keyValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "RoomName",
                table: "Rooms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
