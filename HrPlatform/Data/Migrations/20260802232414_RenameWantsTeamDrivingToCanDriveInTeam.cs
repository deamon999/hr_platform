using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameWantsTeamDrivingToCanDriveInTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WantsTeamDriving",
                table: "DriverProfiles",
                newName: "CanDriveInTeam");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CanDriveInTeam",
                table: "DriverProfiles",
                newName: "WantsTeamDriving");
        }
    }
}
