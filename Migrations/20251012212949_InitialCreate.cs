using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payona.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    StudentNumber = table.Column<string>(type: "text", nullable: true),
                    Department = table.Column<string>(type: "text", nullable: true),
                    Dorm = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fingerprints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealType = table.Column<string>(type: "text", nullable: false),
                    AvailableDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fingerprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fingerprints_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meal_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealType = table.Column<string>(type: "text", nullable: false),
                    PreferredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreferredStartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    PreferredEndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_meal_requests_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FingerprintId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    GiverId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealType = table.Column<string>(type: "text", nullable: false),
                    MatchDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_matches_fingerprints_FingerprintId",
                        column: x => x.FingerprintId,
                        principalTable: "fingerprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_matches_meal_requests_MealRequestId",
                        column: x => x.MealRequestId,
                        principalTable: "meal_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_matches_users_GiverId",
                        column: x => x.GiverId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_matches_users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_messages_matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_messages_users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fingerprints_UserId",
                table: "fingerprints",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_matches_FingerprintId",
                table: "matches",
                column: "FingerprintId");

            migrationBuilder.CreateIndex(
                name: "IX_matches_GiverId",
                table: "matches",
                column: "GiverId");

            migrationBuilder.CreateIndex(
                name: "IX_matches_MealRequestId",
                table: "matches",
                column: "MealRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_matches_ReceiverId",
                table: "matches",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_meal_requests_UserId",
                table: "meal_requests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_MatchId",
                table: "messages",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_ReceiverId",
                table: "messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_SenderId",
                table: "messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "matches");

            migrationBuilder.DropTable(
                name: "fingerprints");

            migrationBuilder.DropTable(
                name: "meal_requests");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
