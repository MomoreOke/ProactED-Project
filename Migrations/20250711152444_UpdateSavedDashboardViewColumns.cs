using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEENALOoFINALE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSavedDashboardViewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SavedDashboardViews");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "SavedDashboardViews",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "FilterData",
                table: "SavedDashboardViews",
                newName: "FilterConfig");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SavedDashboardViews",
                newName: "ViewId");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "SavedDashboardViews",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "SavedDashboardViews");

            migrationBuilder.RenameColumn(
                name: "FilterConfig",
                table: "SavedDashboardViews",
                newName: "FilterData");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "SavedDashboardViews",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "ViewId",
                table: "SavedDashboardViews",
                newName: "Id");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SavedDashboardViews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
