using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class MergeEquipmentExperienceIntoDriverProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentBlobPath",
                table: "DriverMedicalCards");

            migrationBuilder.DropColumn(
                name: "DocumentUploadedAt",
                table: "DriverMedicalCards");

            migrationBuilder.DropColumn(
                name: "DocumentBlobPath",
                table: "DriverLicenses");

            migrationBuilder.DropColumn(
                name: "DocumentUploadedAt",
                table: "DriverLicenses");

            migrationBuilder.AddColumn<int>(
                name: "CarHaulerExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DryVanExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DumpExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FlatbedExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LowboyExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PneumaticExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReeferExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RgnExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StepDeckExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TankerExperience",
                table: "DriverProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Copy existing equipment experience data into DriverProfiles before dropping the table
            migrationBuilder.Sql("""
                UPDATE "DriverProfiles" p SET
                    "DryVanExperience"    = e."DryVan",
                    "ReeferExperience"    = e."Reefer",
                    "FlatbedExperience"   = e."Flatbed",
                    "StepDeckExperience"  = e."StepDeck",
                    "RgnExperience"       = e."Rgn",
                    "LowboyExperience"    = e."Lowboy",
                    "TankerExperience"    = e."Tanker",
                    "CarHaulerExperience" = e."CarHauler",
                    "PneumaticExperience" = e."Pneumatic",
                    "DumpExperience"      = e."Dump"
                FROM "DriverEquipmentExperiences" e
                WHERE e."DriverProfileId" = p."Id";
                """);

            migrationBuilder.DropTable(
                name: "DriverEquipmentExperiences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentBlobPath",
                table: "DriverMedicalCards",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DocumentUploadedAt",
                table: "DriverMedicalCards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentBlobPath",
                table: "DriverLicenses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DocumentUploadedAt",
                table: "DriverLicenses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverEquipmentExperiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverProfileId = table.Column<int>(type: "integer", nullable: false),
                    CarHauler = table.Column<int>(type: "integer", nullable: false),
                    DryVan = table.Column<int>(type: "integer", nullable: false),
                    Dump = table.Column<int>(type: "integer", nullable: false),
                    Flatbed = table.Column<int>(type: "integer", nullable: false),
                    Lowboy = table.Column<int>(type: "integer", nullable: false),
                    Pneumatic = table.Column<int>(type: "integer", nullable: false),
                    Reefer = table.Column<int>(type: "integer", nullable: false),
                    Rgn = table.Column<int>(type: "integer", nullable: false),
                    StepDeck = table.Column<int>(type: "integer", nullable: false),
                    Tanker = table.Column<int>(type: "integer", nullable: false)
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
                name: "IX_DriverEquipmentExperiences_DriverProfileId",
                table: "DriverEquipmentExperiences",
                column: "DriverProfileId",
                unique: true);

            // Restore equipment experience rows from the merged DriverProfiles columns
            migrationBuilder.Sql("""
                INSERT INTO "DriverEquipmentExperiences"
                    ("DriverProfileId", "DryVan", "Reefer", "Flatbed", "StepDeck", "Rgn", "Lowboy", "Tanker", "CarHauler", "Pneumatic", "Dump")
                SELECT "Id", "DryVanExperience", "ReeferExperience", "FlatbedExperience", "StepDeckExperience", "RgnExperience",
                       "LowboyExperience", "TankerExperience", "CarHaulerExperience", "PneumaticExperience", "DumpExperience"
                FROM "DriverProfiles";
                """);

            migrationBuilder.DropColumn(
                name: "CarHaulerExperience",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "DryVanExperience",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "DumpExperience",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "FlatbedExperience",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "LowboyExperience",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "PneumaticExperience",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "ReeferExperience",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "RgnExperience",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "StepDeckExperience",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "TankerExperience",
                table: "DriverProfiles");
        }
    }
}
