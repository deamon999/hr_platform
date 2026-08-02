using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTermsAcceptedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TermsAccepted",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TermsAcceptedDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TermsAccepted",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TermsAcceptedDate",
                table: "AspNetUsers");
        }
    }
}
