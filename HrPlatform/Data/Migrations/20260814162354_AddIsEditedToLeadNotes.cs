using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsEditedToLeadNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEdited",
                table: "LeadNotes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEdited",
                table: "LeadNotes");
        }
    }
}
