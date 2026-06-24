using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedCompanyProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BenefitsOffered",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BenefitsSummary",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FleetSize",
                table: "Companies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HiringNewGrads",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HomeTime",
                table: "Companies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaidCdlTraining",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SignOnBonus",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Companies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BenefitsOffered",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "BenefitsSummary",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "FleetSize",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "HiringNewGrads",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "HomeTime",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PaidCdlTraining",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SignOnBonus",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Companies");
        }
    }
}
