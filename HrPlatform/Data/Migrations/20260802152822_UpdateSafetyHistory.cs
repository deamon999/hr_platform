using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSafetyHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCompletedSAPProgram",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasFailedDrugTest",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLicenseSuspension",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasRefusedDrugTest",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCompletedSAPProgram",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "HasFailedDrugTest",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "HasLicenseSuspension",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "HasRefusedDrugTest",
                table: "DriverProfiles");
        }
    }
}
