using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebProject_API_React.Server.Migrations
{
    /// <inheritdoc />
    public partial class BackgroundTasksWithParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Parameters",
                table: "BackgroundTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 11, 5, 22, 56, 49, 782, DateTimeKind.Utc).AddTicks(2720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 11, 5, 22, 56, 49, 782, DateTimeKind.Utc).AddTicks(2725));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 11, 5, 22, 56, 49, 782, DateTimeKind.Utc).AddTicks(2727));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Parameters",
                table: "BackgroundTasks");

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
        }
    }
}
