using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class MergeDocumentFileAndDriverDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverDocuments");

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "DocumentFiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverProfileId",
                table: "DocumentFiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DocumentFiles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFiles_DriverProfileId",
                table: "DocumentFiles",
                column: "DriverProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentFiles_DriverProfiles_DriverProfileId",
                table: "DocumentFiles",
                column: "DriverProfileId",
                principalTable: "DriverProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentFiles_DriverProfiles_DriverProfileId",
                table: "DocumentFiles");

            migrationBuilder.DropIndex(
                name: "IX_DocumentFiles_DriverProfileId",
                table: "DocumentFiles");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "DocumentFiles");

            migrationBuilder.DropColumn(
                name: "DriverProfileId",
                table: "DocumentFiles");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DocumentFiles");

            migrationBuilder.CreateTable(
                name: "DriverDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentFileId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DriverProfileId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverDocuments_DocumentFiles_DocumentFileId",
                        column: x => x.DocumentFileId,
                        principalTable: "DocumentFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DriverDocuments_DriverProfiles_DriverProfileId",
                        column: x => x.DriverProfileId,
                        principalTable: "DriverProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverDocuments_DocumentFileId",
                table: "DriverDocuments",
                column: "DocumentFileId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverDocuments_DriverProfileId",
                table: "DriverDocuments",
                column: "DriverProfileId");
        }
    }
}
