using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDocumentFileDataColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leads_CompanyId_Email",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_CompanyId_Phone",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "DocumentFiles");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CompanyId_Email",
                table: "Leads",
                columns: new[] { "CompanyId", "Email" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CompanyId_Phone",
                table: "Leads",
                columns: new[] { "CompanyId", "Phone" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leads_CompanyId_Email",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_CompanyId_Phone",
                table: "Leads");

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "DocumentFiles",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CompanyId_Email",
                table: "Leads",
                columns: new[] { "CompanyId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CompanyId_Phone",
                table: "Leads",
                columns: new[] { "CompanyId", "Phone" },
                unique: true);
        }
    }
}
