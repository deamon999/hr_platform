using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentUploadsDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Companies_CompanyId1",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompanyId1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyId1",
                table: "AspNetUsers");

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

            migrationBuilder.AddColumn<string>(
                name: "DocumentBlobPath",
                table: "DriverCertifications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DocumentUploadedAt",
                table: "DriverCertifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentFiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentFiles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentFiles");

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

            migrationBuilder.DropColumn(
                name: "DocumentBlobPath",
                table: "DriverCertifications");

            migrationBuilder.DropColumn(
                name: "DocumentUploadedAt",
                table: "DriverCertifications");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId1",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId1",
                table: "AspNetUsers",
                column: "CompanyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Companies_CompanyId1",
                table: "AspNetUsers",
                column: "CompanyId1",
                principalTable: "Companies",
                principalColumn: "Id");
        }
    }
}
