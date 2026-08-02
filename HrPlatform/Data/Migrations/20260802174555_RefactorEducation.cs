using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HrPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorEducation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverEducations");

            migrationBuilder.AddColumn<string>(
                name: "EducationCity",
                table: "DriverProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationGraduationYear",
                table: "DriverProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationSchoolName",
                table: "DriverProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationState",
                table: "DriverProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighestEducationLevel",
                table: "DriverProfiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationCity",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "EducationGraduationYear",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "EducationSchoolName",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "EducationState",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "HighestEducationLevel",
                table: "DriverProfiles");

            migrationBuilder.CreateTable(
                name: "DriverEducations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverProfileId = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FieldOfStudy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Graduated = table.Column<bool>(type: "boolean", nullable: false),
                    GraduationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InstitutionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Level = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverEducations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverEducations_DriverProfiles_DriverProfileId",
                        column: x => x.DriverProfileId,
                        principalTable: "DriverProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverEducations_DriverProfileId",
                table: "DriverEducations",
                column: "DriverProfileId");
        }
    }
}
