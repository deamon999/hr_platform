using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvitationIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Invitations",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_Phone",
                table: "Invitations",
                column: "Phone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invitations_Phone",
                table: "Invitations");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Invitations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);
        }
    }
}
