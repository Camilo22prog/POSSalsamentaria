using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCampoIVAProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Cajas_CajaId",
                table: "MovimientosCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                table: "MovimientosCaja");

            migrationBuilder.AddColumn<int>(
                name: "TipoIVA",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "MovimientosCaja",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "MovimientosCaja",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Concepto",
                table: "MovimientosCaja",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Cajas_CajaId",
                table: "MovimientosCaja",
                column: "CajaId",
                principalTable: "Cajas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                table: "MovimientosCaja",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Cajas_CajaId",
                table: "MovimientosCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                table: "MovimientosCaja");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosCaja_Fecha",
                table: "MovimientosCaja");

            migrationBuilder.DropColumn(
                name: "TipoIVA",
                table: "Productos");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "MovimientosCaja",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "MovimientosCaja",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Concepto",
                table: "MovimientosCaja",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 12, 13, 49, 45, 423, DateTimeKind.Local).AddTicks(5681));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 12, 13, 49, 45, 423, DateTimeKind.Local).AddTicks(5684));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 12, 13, 49, 45, 423, DateTimeKind.Local).AddTicks(5685));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 12, 13, 49, 45, 423, DateTimeKind.Local).AddTicks(5687));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 12, 13, 49, 45, 423, DateTimeKind.Local).AddTicks(5688));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 12, 13, 49, 45, 425, DateTimeKind.Local).AddTicks(8966));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 12, 13, 49, 45, 425, DateTimeKind.Local).AddTicks(8969));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 2, 12, 13, 49, 45, 426, DateTimeKind.Local).AddTicks(2066));

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Cajas_CajaId",
                table: "MovimientosCaja",
                column: "CajaId",
                principalTable: "Cajas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Usuarios_UsuarioId",
                table: "MovimientosCaja",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
