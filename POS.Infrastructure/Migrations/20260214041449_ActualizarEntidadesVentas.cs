using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarEntidadesVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosCaja_Fecha",
                table: "MovimientosCaja");

            migrationBuilder.DropIndex(
                name: "IX_Cajas_FechaApertura",
                table: "Cajas");

            migrationBuilder.DropIndex(
                name: "IX_Cajas_NumeroCaja",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "AnuladaPorId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "MovimientosCaja");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "PesoVendido",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "PrecioFinal",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "NumeroCaja",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "ObservacionesCierre",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "TotalOtros",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "TotalVentas",
                table: "Cajas");

            migrationBuilder.RenameColumn(
                name: "Impuesto",
                table: "Ventas",
                newName: "IVATotal");

            migrationBuilder.RenameColumn(
                name: "Descuento",
                table: "Ventas",
                newName: "DescuentoTotal");

            migrationBuilder.RenameColumn(
                name: "MetodoPago",
                table: "Pagos",
                newName: "Metodo");

            migrationBuilder.AlterColumn<int>(
                name: "Tipo",
                table: "MovimientosCaja",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Concepto",
                table: "MovimientosCaja",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Referencia",
                table: "MovimientosCaja",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoIVA",
                table: "DetallesVenta",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeIVA",
                table: "DetallesVenta",
                type: "decimal(18,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "DetallesVenta",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDaviplata",
                table: "Cajas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNequi",
                table: "Cajas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalQR",
                table: "Cajas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId1",
                table: "Cajas",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 23, 14, 47, 909, DateTimeKind.Local).AddTicks(7866));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 23, 14, 47, 909, DateTimeKind.Local).AddTicks(7870));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 23, 14, 47, 909, DateTimeKind.Local).AddTicks(7873));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 23, 14, 47, 909, DateTimeKind.Local).AddTicks(7876));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 23, 14, 47, 909, DateTimeKind.Local).AddTicks(7879));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 23, 14, 47, 916, DateTimeKind.Local).AddTicks(9512));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 23, 14, 47, 916, DateTimeKind.Local).AddTicks(9519));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 23, 14, 47, 917, DateTimeKind.Local).AddTicks(6565));

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_UsuarioId1",
                table: "Cajas",
                column: "UsuarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Cajas_Usuarios_UsuarioId1",
                table: "Cajas",
                column: "UsuarioId1",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cajas_Usuarios_UsuarioId1",
                table: "Cajas");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta");

            migrationBuilder.DropIndex(
                name: "IX_Cajas_UsuarioId1",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "Referencia",
                table: "MovimientosCaja");

            migrationBuilder.DropColumn(
                name: "MontoIVA",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "PorcentajeIVA",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "DetallesVenta");

            migrationBuilder.DropColumn(
                name: "TotalDaviplata",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "TotalNequi",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "TotalQR",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "Cajas");

            migrationBuilder.RenameColumn(
                name: "IVATotal",
                table: "Ventas",
                newName: "Impuesto");

            migrationBuilder.RenameColumn(
                name: "DescuentoTotal",
                table: "Ventas",
                newName: "Descuento");

            migrationBuilder.RenameColumn(
                name: "Metodo",
                table: "Pagos",
                newName: "MetodoPago");

            migrationBuilder.AddColumn<int>(
                name: "AnuladaPorId",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "Ventas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "MovimientosCaja",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Concepto",
                table: "MovimientosCaja",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "MovimientosCaja",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "DetallesVenta",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PesoVendido",
                table: "DetallesVenta",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioFinal",
                table: "DetallesVenta",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NumeroCaja",
                table: "Cajas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesCierre",
                table: "Cajas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalOtros",
                table: "Cajas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalVentas",
                table: "Cajas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 8, 17, 38, 713, DateTimeKind.Local).AddTicks(1766));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 8, 17, 38, 713, DateTimeKind.Local).AddTicks(1768));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 8, 17, 38, 713, DateTimeKind.Local).AddTicks(1770));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 8, 17, 38, 713, DateTimeKind.Local).AddTicks(1771));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 8, 17, 38, 713, DateTimeKind.Local).AddTicks(1772));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 8, 17, 38, 716, DateTimeKind.Local).AddTicks(4785));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 8, 17, 38, 716, DateTimeKind.Local).AddTicks(4788));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 13, 8, 17, 38, 716, DateTimeKind.Local).AddTicks(8093));

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_Fecha",
                table: "MovimientosCaja",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_FechaApertura",
                table: "Cajas",
                column: "FechaApertura");

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_NumeroCaja",
                table: "Cajas",
                column: "NumeroCaja");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesVenta_Ventas_VentaId",
                table: "DetallesVenta",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
