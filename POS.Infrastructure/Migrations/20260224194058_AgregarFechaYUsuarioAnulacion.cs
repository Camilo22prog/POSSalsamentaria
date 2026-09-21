using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaYUsuarioAnulacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnuladaPorUsuarioId",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "Ventas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 14, 40, 57, 513, DateTimeKind.Local).AddTicks(2164));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 14, 40, 57, 513, DateTimeKind.Local).AddTicks(2173));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 14, 40, 57, 513, DateTimeKind.Local).AddTicks(2179));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 14, 40, 57, 513, DateTimeKind.Local).AddTicks(2183));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 14, 40, 57, 513, DateTimeKind.Local).AddTicks(2187));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 14, 40, 57, 531, DateTimeKind.Local).AddTicks(8759));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 14, 40, 57, 531, DateTimeKind.Local).AddTicks(8770));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 14, 40, 57, 532, DateTimeKind.Local).AddTicks(9605));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnuladaPorUsuarioId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "Ventas");

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 9, 7, 12, 453, DateTimeKind.Local).AddTicks(7972));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 9, 7, 12, 453, DateTimeKind.Local).AddTicks(7977));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 9, 7, 12, 453, DateTimeKind.Local).AddTicks(7981));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 9, 7, 12, 453, DateTimeKind.Local).AddTicks(7984));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 9, 7, 12, 453, DateTimeKind.Local).AddTicks(7986));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 9, 7, 12, 463, DateTimeKind.Local).AddTicks(4701));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 9, 7, 12, 463, DateTimeKind.Local).AddTicks(4709));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 24, 9, 7, 12, 464, DateTimeKind.Local).AddTicks(2770));
        }
    }
}
