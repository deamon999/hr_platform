using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOtherExperience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutomaticTransmission",
                table: "DriverEquipmentExperiences");

            migrationBuilder.DropColumn(
                name: "CanadaExperience",
                table: "DriverEquipmentExperiences");

            migrationBuilder.DropColumn(
                name: "HazmatEndorsement",
                table: "DriverEquipmentExperiences");

            migrationBuilder.DropColumn(
                name: "MountainDriving",
                table: "DriverEquipmentExperiences");

            migrationBuilder.DropColumn(
                name: "NycExperience",
                table: "DriverEquipmentExperiences");

            migrationBuilder.DropColumn(
                name: "WinterDriving",
                table: "DriverEquipmentExperiences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutomaticTransmission",
                table: "DriverEquipmentExperiences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanadaExperience",
                table: "DriverEquipmentExperiences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HazmatEndorsement",
                table: "DriverEquipmentExperiences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MountainDriving",
                table: "DriverEquipmentExperiences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NycExperience",
                table: "DriverEquipmentExperiences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WinterDriving",
                table: "DriverEquipmentExperiences",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
