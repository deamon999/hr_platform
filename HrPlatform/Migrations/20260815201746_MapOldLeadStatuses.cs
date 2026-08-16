using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class MapOldLeadStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Leads\" SET \"Status\" = 'AttemptContact' WHERE \"Status\" = 'Contacted';");
            migrationBuilder.Sql("UPDATE \"Leads\" SET \"Status\" = 'NotInterested' WHERE \"Status\" = 'Rejected';");
            migrationBuilder.Sql("UPDATE \"Leads\" SET \"Status\" = 'AttemptContact' WHERE \"Status\" = 'Invited';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
