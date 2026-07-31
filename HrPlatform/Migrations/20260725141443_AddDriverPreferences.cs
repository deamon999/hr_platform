using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanDriveManual",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PreferredHomeTime",
                table: "DriverProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WantsTeamDriving",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WantsToDriveWithPets",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WantsToDriveWithRiders",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanDriveManual",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredHomeTime",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "WantsTeamDriving",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "WantsToDriveWithPets",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "WantsToDriveWithRiders",
                table: "DriverProfiles");
        }
    }
}
