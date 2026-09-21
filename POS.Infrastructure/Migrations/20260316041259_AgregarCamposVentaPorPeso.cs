using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposVentaPorPeso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoriaPeso",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioPorKilo",
                table: "Productos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "VentaPorPeso",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 15, 23, 12, 58, 870, DateTimeKind.Local).AddTicks(5225));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 15, 23, 12, 58, 870, DateTimeKind.Local).AddTicks(5230));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 15, 23, 12, 58, 870, DateTimeKind.Local).AddTicks(5233));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 15, 23, 12, 58, 870, DateTimeKind.Local).AddTicks(5235));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 15, 23, 12, 58, 870, DateTimeKind.Local).AddTicks(5237));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 15, 23, 12, 58, 877, DateTimeKind.Local).AddTicks(8792));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 15, 23, 12, 58, 877, DateTimeKind.Local).AddTicks(8798));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 15, 23, 12, 58, 878, DateTimeKind.Local).AddTicks(4761));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriaPeso",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioPorKilo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "VentaPorPeso",
                table: "Productos");

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
    }
}
