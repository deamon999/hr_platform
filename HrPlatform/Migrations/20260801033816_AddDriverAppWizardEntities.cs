using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverAppWizardEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConsentClearinghouse",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentEmployment",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentFCRA",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentMVR",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentPSP",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ElectronicSignatureName",
                table: "DriverProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasMilitaryService",
                table: "DriverProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MilitaryBranch",
                table: "DriverProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MilitaryYears",
                table: "DriverProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MinimumWeeklyPay",
                table: "DriverProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "PreferredFreight",
                table: "DriverProfiles",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredPosition",
                table: "DriverProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "PreferredRegions",
                table: "DriverProfiles",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SignatureDate",
                table: "DriverProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverProfileId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverDocuments_DriverProfiles_DriverProfileId",
                        column: x => x.DriverProfileId,
                        principalTable: "DriverProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverEquipmentExperiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverProfileId = table.Column<int>(type: "integer", nullable: false),
                    DryVan = table.Column<int>(type: "integer", nullable: false),
                    Reefer = table.Column<int>(type: "integer", nullable: false),
                    Flatbed = table.Column<int>(type: "integer", nullable: false),
                    StepDeck = table.Column<int>(type: "integer", nullable: false),
                    Rgn = table.Column<int>(type: "integer", nullable: false),
                    Lowboy = table.Column<int>(type: "integer", nullable: false),
                    Tanker = table.Column<int>(type: "integer", nullable: false),
                    CarHauler = table.Column<int>(type: "integer", nullable: false),
                    Pneumatic = table.Column<int>(type: "integer", nullable: false),
                    Dump = table.Column<int>(type: "integer", nullable: false),
                    AutomaticTransmission = table.Column<bool>(type: "boolean", nullable: false),
                    CanadaExperience = table.Column<bool>(type: "boolean", nullable: false),
                    HazmatEndorsement = table.Column<bool>(type: "boolean", nullable: false),
                    MountainDriving = table.Column<bool>(type: "boolean", nullable: false),
                    WinterDriving = table.Column<bool>(type: "boolean", nullable: false),
                    NycExperience = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverEquipmentExperiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverEquipmentExperiences_DriverProfiles_DriverProfileId",
                        column: x => x.DriverProfileId,
                        principalTable: "DriverProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverDocuments_DriverProfileId",
                table: "DriverDocuments",
                column: "DriverProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverEquipmentExperiences_DriverProfileId",
                table: "DriverEquipmentExperiences",
                column: "DriverProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverDocuments");

            migrationBuilder.DropTable(
                name: "DriverEquipmentExperiences");

            migrationBuilder.DropColumn(
                name: "ConsentClearinghouse",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "ConsentEmployment",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "ConsentFCRA",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "ConsentMVR",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "ConsentPSP",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "ElectronicSignatureName",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "HasMilitaryService",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "MilitaryBranch",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "MilitaryYears",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "MinimumWeeklyPay",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredFreight",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredPosition",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredRegions",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "SignatureDate",
                table: "DriverProfiles");
        }
    }
}
