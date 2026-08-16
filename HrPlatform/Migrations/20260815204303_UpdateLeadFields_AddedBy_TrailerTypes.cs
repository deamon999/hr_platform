using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLeadFields_AddedBy_TrailerTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TrailerType",
                table: "Leads");

            migrationBuilder.AddColumn<string>(
                name: "AddedByUserId",
                table: "Leads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "TrailerTypes",
                table: "Leads",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_AddedByUserId",
                table: "Leads",
                column: "AddedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_AspNetUsers_AddedByUserId",
                table: "Leads",
                column: "AddedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_AspNetUsers_AddedByUserId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_AddedByUserId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "AddedByUserId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "TrailerTypes",
                table: "Leads");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Leads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TrailerType",
                table: "Leads",
                type: "integer",
                nullable: true);
        }
    }
}
