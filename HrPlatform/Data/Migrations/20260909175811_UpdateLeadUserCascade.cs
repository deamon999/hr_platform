using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLeadUserCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadNotes_AspNetUsers_AuthorUserId",
                table: "LeadNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_AspNetUsers_AddedByUserId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_CompanyId_Email",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_CompanyId_Phone",
                table: "Leads");

            migrationBuilder.AlterColumn<string>(
                name: "AuthorUserId",
                table: "LeadNotes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CompanyId_Email",
                table: "Leads",
                columns: new[] { "CompanyId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CompanyId_Phone",
                table: "Leads",
                columns: new[] { "CompanyId", "Phone" });

            migrationBuilder.AddForeignKey(
                name: "FK_LeadNotes_AspNetUsers_AuthorUserId",
                table: "LeadNotes",
                column: "AuthorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_AspNetUsers_AddedByUserId",
                table: "Leads",
                column: "AddedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadNotes_AspNetUsers_AuthorUserId",
                table: "LeadNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_AspNetUsers_AddedByUserId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_CompanyId_Email",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_CompanyId_Phone",
                table: "Leads");

            migrationBuilder.AlterColumn<string>(
                name: "AuthorUserId",
                table: "LeadNotes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_LeadNotes_AspNetUsers_AuthorUserId",
                table: "LeadNotes",
                column: "AuthorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_AspNetUsers_AddedByUserId",
                table: "Leads",
                column: "AddedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
