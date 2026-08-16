using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMedicalCardSelfCertification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelfCertified",
                table: "DriverMedicalCards");

            migrationBuilder.AddColumn<int>(
                name: "SelfCertification",
                table: "DriverMedicalCards",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelfCertification",
                table: "DriverMedicalCards");

            migrationBuilder.AddColumn<bool>(
                name: "SelfCertified",
                table: "DriverMedicalCards",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
