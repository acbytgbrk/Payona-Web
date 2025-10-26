using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payona.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStudentNumberAndDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "users");

            migrationBuilder.DropColumn(
                name: "StudentNumber",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentNumber",
                table: "users",
                type: "text",
                nullable: true);
        }
    }
}
