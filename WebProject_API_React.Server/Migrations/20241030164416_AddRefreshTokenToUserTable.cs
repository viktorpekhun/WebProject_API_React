using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebProject.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenToUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "RefreshToken", "RefreshTokenExpiryTime" },
                values: new object[] { "token", new DateTime(2024, 10, 30, 18, 44, 16, 302, DateTimeKind.Utc).AddTicks(5169) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "RefreshToken", "RefreshTokenExpiryTime" },
                values: new object[] { "token", new DateTime(2024, 10, 30, 18, 44, 16, 302, DateTimeKind.Utc).AddTicks(5174) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "RefreshToken", "RefreshTokenExpiryTime" },
                values: new object[] { "token", new DateTime(2024, 10, 30, 18, 44, 16, 302, DateTimeKind.Utc).AddTicks(5176) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Users");
        }
    }
}
