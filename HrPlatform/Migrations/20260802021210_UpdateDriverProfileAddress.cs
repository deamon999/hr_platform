using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDriverProfileAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "DriverProfiles");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "DriverProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "DriverProfiles");

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "DriverProfiles",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }
    }
}
