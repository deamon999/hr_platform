using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmploymentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Responsibilities",
                table: "DriverEmployments");

            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "DriverEmployments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyPhone",
                table: "DriverEmployments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MayWeContact",
                table: "DriverEmployments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PayType",
                table: "DriverEmployments",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyEmail",
                table: "DriverEmployments");

            migrationBuilder.DropColumn(
                name: "CompanyPhone",
                table: "DriverEmployments");

            migrationBuilder.DropColumn(
                name: "MayWeContact",
                table: "DriverEmployments");

            migrationBuilder.DropColumn(
                name: "PayType",
                table: "DriverEmployments");

            migrationBuilder.AddColumn<string>(
                name: "Responsibilities",
                table: "DriverEmployments",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
