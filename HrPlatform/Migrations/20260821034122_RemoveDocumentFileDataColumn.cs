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
            migrationBuilder.DropColumn(
                name: "Data",
                table: "DocumentFiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "DocumentFiles",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
