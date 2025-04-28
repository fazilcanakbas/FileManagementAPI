using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class mg00 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b5b62c7a-6241-4d8a-81c6-f4f63f123abc",
                column: "CreatedAt",
                value: new DateTime(2025, 4, 25, 12, 30, 8, 836, DateTimeKind.Utc).AddTicks(47));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e3b4f7d1-1de3-4b4a-a648-52ff5f4c0b23",
                column: "CreatedAt",
                value: new DateTime(2025, 4, 25, 12, 30, 8, 837, DateTimeKind.Utc).AddTicks(8265));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b5b62c7a-6241-4d8a-81c6-f4f63f123abc",
                column: "CreatedAt",
                value: new DateTime(2025, 4, 25, 12, 22, 26, 681, DateTimeKind.Utc).AddTicks(5691));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e3b4f7d1-1de3-4b4a-a648-52ff5f4c0b23",
                column: "CreatedAt",
                value: new DateTime(2025, 4, 25, 12, 22, 26, 684, DateTimeKind.Utc).AddTicks(2273));
        }
    }
}
