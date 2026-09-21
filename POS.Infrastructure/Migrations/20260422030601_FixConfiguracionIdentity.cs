using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixConfiguracionIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server cannot ALTER an IDENTITY column — must drop and recreate
            migrationBuilder.DropTable(name: "Configuraciones");

            migrationBuilder.CreateTable(
                name: "Configuraciones",
                columns: table => new
                {
                    Id                   = table.Column<int>(type: "int", nullable: false),
                    NombreNegocio        = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nit                  = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion            = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono             = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MensajePieTicket     = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreImpresora      = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AbrirCajonAutomatico = table.Column<bool>(type: "bit", nullable: false),
                    PuertoBalanza        = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaudRateBalanza      = table.Column<int>(type: "int", nullable: false),
                    UrlActualizaciones   = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion    = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuraciones", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 21, 22, 6, 0, 399, DateTimeKind.Local).AddTicks(8139));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 21, 22, 6, 0, 399, DateTimeKind.Local).AddTicks(8146));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 21, 22, 6, 0, 399, DateTimeKind.Local).AddTicks(8152));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 21, 22, 6, 0, 399, DateTimeKind.Local).AddTicks(8159));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 21, 22, 6, 0, 399, DateTimeKind.Local).AddTicks(8164));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Configuraciones");

            migrationBuilder.CreateTable(
                name: "Configuraciones",
                columns: table => new
                {
                    Id                   = table.Column<int>(type: "int", nullable: false)
                                               .Annotation("SqlServer:Identity", "1, 1"),
                    NombreNegocio        = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nit                  = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion            = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono             = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MensajePieTicket     = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreImpresora      = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AbrirCajonAutomatico = table.Column<bool>(type: "bit", nullable: false),
                    PuertoBalanza        = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaudRateBalanza      = table.Column<int>(type: "int", nullable: false),
                    UrlActualizaciones   = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion    = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuraciones", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 21, 22, 0, 33, 520, DateTimeKind.Local).AddTicks(8947));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 21, 22, 0, 33, 520, DateTimeKind.Local).AddTicks(8952));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 21, 22, 0, 33, 520, DateTimeKind.Local).AddTicks(8957));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 21, 22, 0, 33, 520, DateTimeKind.Local).AddTicks(8961));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 21, 22, 0, 33, 520, DateTimeKind.Local).AddTicks(8965));
        }
    }
}
