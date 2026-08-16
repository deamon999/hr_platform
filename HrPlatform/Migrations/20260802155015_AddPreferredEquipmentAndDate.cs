using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredEquipmentAndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "AvailableStartDate",
                table: "DriverProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredEquipment",
                table: "DriverProfiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableStartDate",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredEquipment",
                table: "DriverProfiles");
        }
    }
}
