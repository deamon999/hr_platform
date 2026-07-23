using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddTruckingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowsPets",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsRiders",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HomeTime",
                table: "Jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTeamDriving",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresManualTransmission",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SignOnBonus",
                table: "Jobs",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowsPets",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AllowsRiders",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "HomeTime",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsTeamDriving",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequiresManualTransmission",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SignOnBonus",
                table: "Jobs");
        }
    }
}
