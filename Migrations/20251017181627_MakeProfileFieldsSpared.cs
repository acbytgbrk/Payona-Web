using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payona.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeProfileFieldsSpared : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Dorm",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "users");

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Dorm = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dorm",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "users",
                type: "text",
                nullable: true);
        }
    }
}
