using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenseSuspensionDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverCertifications");

            migrationBuilder.AddColumn<DateOnly>(
                name: "LicenseSuspensionDate",
                table: "DriverProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseSuspensionReason",
                table: "DriverProfiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicenseSuspensionDate",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "LicenseSuspensionReason",
                table: "DriverProfiles");

            migrationBuilder.CreateTable(
                name: "DriverCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverProfileId = table.Column<int>(type: "integer", nullable: false),
                    CertificationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentBlobPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DocumentUploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IssuingAuthority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverCertifications_DriverProfiles_DriverProfileId",
                        column: x => x.DriverProfileId,
                        principalTable: "DriverProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverCertifications_DriverProfileId",
                table: "DriverCertifications",
                column: "DriverProfileId");
        }
    }
}
