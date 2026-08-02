using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameHomeTimeToRouteType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HomeTime",
                table: "Jobs",
                newName: "RouteType");

            migrationBuilder.RenameColumn(
                name: "PreferredHomeTime",
                table: "DriverProfiles",
                newName: "PreferredRouteType");

            migrationBuilder.RenameColumn(
                name: "HomeTime",
                table: "Companies",
                newName: "RouteType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RouteType",
                table: "Jobs",
                newName: "HomeTime");

            migrationBuilder.RenameColumn(
                name: "PreferredRouteType",
                table: "DriverProfiles",
                newName: "PreferredHomeTime");

            migrationBuilder.RenameColumn(
                name: "RouteType",
                table: "Companies",
                newName: "HomeTime");
        }
    }
}
