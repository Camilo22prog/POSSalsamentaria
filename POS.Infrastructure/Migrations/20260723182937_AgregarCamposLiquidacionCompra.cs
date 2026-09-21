using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposLiquidacionCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActualizarCatalogo",
                table: "DetallesCompra",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoUnitarioLiquidado",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoPorcentaje",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoValor",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IbuaPorcentaje",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IbuaValor",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IcuiPorcentaje",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IcuiValor",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IvaPorcentaje",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IvaValor",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LoteCodigo",
                table: "DetallesCompra",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeGanancia",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioConIva",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioSinIva",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVentaCalculado",
                table: "DetallesCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 7, 23, 13, 29, 36, 838, DateTimeKind.Local).AddTicks(8920));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 7, 23, 13, 29, 36, 838, DateTimeKind.Local).AddTicks(8924));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 7, 23, 13, 29, 36, 838, DateTimeKind.Local).AddTicks(8928));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 7, 23, 13, 29, 36, 838, DateTimeKind.Local).AddTicks(8931));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 7, 23, 13, 29, 36, 838, DateTimeKind.Local).AddTicks(8934));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualizarCatalogo",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "CostoUnitarioLiquidado",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "DescuentoPorcentaje",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "DescuentoValor",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "IbuaPorcentaje",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "IbuaValor",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "IcuiPorcentaje",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "IcuiValor",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "IvaPorcentaje",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "IvaValor",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "LoteCodigo",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "PorcentajeGanancia",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "PrecioConIva",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "PrecioSinIva",
                table: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "PrecioVentaCalculado",
                table: "DetallesCompra");

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 7, 1, 15, 1, 46, 805, DateTimeKind.Local).AddTicks(1165));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 7, 1, 15, 1, 46, 805, DateTimeKind.Local).AddTicks(1167));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 7, 1, 15, 1, 46, 805, DateTimeKind.Local).AddTicks(1169));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 7, 1, 15, 1, 46, 805, DateTimeKind.Local).AddTicks(1170));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 7, 1, 15, 1, 46, 805, DateTimeKind.Local).AddTicks(1172));
        }
    }
}
