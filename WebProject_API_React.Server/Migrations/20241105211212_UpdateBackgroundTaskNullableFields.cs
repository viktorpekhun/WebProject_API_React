using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebProject_API_React.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBackgroundTaskNullableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "BackgroundTasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "BackgroundTasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 11, 5, 23, 12, 11, 991, DateTimeKind.Utc).AddTicks(9135));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 11, 5, 23, 12, 11, 991, DateTimeKind.Utc).AddTicks(9140));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "RefreshTokenExpiryTime",
                value: new DateTime(2024, 11, 5, 23, 12, 11, 991, DateTimeKind.Utc).AddTicks(9142));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "BackgroundTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "BackgroundTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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
    }
}
