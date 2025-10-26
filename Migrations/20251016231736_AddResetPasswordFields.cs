using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payona.API.Migrations
{
    /// <inheritdoc />
    public partial class AddResetPasswordFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordExpires",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordToken",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetPasswordExpires",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ResetPasswordToken",
                table: "users");
        }
    }
}
