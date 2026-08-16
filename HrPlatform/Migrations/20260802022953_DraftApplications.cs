using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class DraftApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableFrom",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "ProfessionalSummary",
                table: "DriverProfiles");

            migrationBuilder.RenameColumn(
                name: "AvailabilityStatus",
                table: "DriverProfiles",
                newName: "LastWizardStep");

            migrationBuilder.AddColumn<bool>(
                name: "IsApplicationCompleted",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApplicationCompleted",
                table: "DriverProfiles");

            migrationBuilder.RenameColumn(
                name: "LastWizardStep",
                table: "DriverProfiles",
                newName: "AvailabilityStatus");

            migrationBuilder.AddColumn<DateOnly>(
                name: "AvailableFrom",
                table: "DriverProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfessionalSummary",
                table: "DriverProfiles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
