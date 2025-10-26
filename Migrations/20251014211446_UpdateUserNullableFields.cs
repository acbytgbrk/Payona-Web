using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payona.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserNullableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "users",
                newName: "City");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "City",
                table: "users",
                newName: "Phone");
        }
    }
}
