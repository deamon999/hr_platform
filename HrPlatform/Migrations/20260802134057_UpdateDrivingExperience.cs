using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDrivingExperience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccidentFreeMiles",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "TotalMilesDriven",
                table: "DriverProfiles");

            migrationBuilder.RenameColumn(
                name: "StatesOperated",
                table: "DriverProfiles",
                newName: "OwnerOperatorExperience");

            migrationBuilder.RenameColumn(
                name: "AverageWeeklyMiles",
                table: "DriverProfiles",
                newName: "OtrExperience");

            migrationBuilder.AddColumn<int>(
                name: "LocalExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "SelfCertification",
                table: "DriverMedicalCards",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalExperience",
                table: "DriverProfiles");

            migrationBuilder.RenameColumn(
                name: "OwnerOperatorExperience",
                table: "DriverProfiles",
                newName: "StatesOperated");

            migrationBuilder.RenameColumn(
                name: "OtrExperience",
                table: "DriverProfiles",
                newName: "AverageWeeklyMiles");

            migrationBuilder.AddColumn<long>(
                name: "AccidentFreeMiles",
                table: "DriverProfiles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TotalMilesDriven",
                table: "DriverProfiles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<int>(
                name: "SelfCertification",
                table: "DriverMedicalCards",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
