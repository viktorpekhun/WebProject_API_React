using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebProject_API_React.Server.Migrations
{
    /// <inheritdoc />
    public partial class BackgroundTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackgroundTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TaskType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackgroundTasks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 11, 5, 20, 0, 7, 936, DateTimeKind.Utc).AddTicks(2431));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 11, 5, 20, 0, 7, 936, DateTimeKind.Utc).AddTicks(2435));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 11, 5, 20, 0, 7, 936, DateTimeKind.Utc).AddTicks(2438));

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundTasks_UserId",
                table: "BackgroundTasks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackgroundTasks");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 10, 30, 18, 44, 16, 302, DateTimeKind.Utc).AddTicks(5169));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 10, 30, 18, 44, 16, 302, DateTimeKind.Utc).AddTicks(5174));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 10, 30, 18, 44, 16, 302, DateTimeKind.Utc).AddTicks(5176));
        }
    }
}
