using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class LinkDocumentFileToDriverDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "DriverDocuments",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "DocumentFileId",
                table: "DriverDocuments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverDocuments_DocumentFileId",
                table: "DriverDocuments",
                column: "DocumentFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverDocuments_DocumentFiles_DocumentFileId",
                table: "DriverDocuments",
                column: "DocumentFileId",
                principalTable: "DocumentFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverDocuments_DocumentFiles_DocumentFileId",
                table: "DriverDocuments");

            migrationBuilder.DropIndex(
                name: "IX_DriverDocuments_DocumentFileId",
                table: "DriverDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentFileId",
                table: "DriverDocuments");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "DriverDocuments",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
