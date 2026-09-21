using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProveedorAProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProveedorId",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_ProveedorId",
                table: "Productos",
                column: "ProveedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Proveedores_ProveedorId",
                table: "Productos",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Proveedores_ProveedorId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_ProveedorId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ProveedorId",
                table: "Productos");

            migrationBuilder.RenameColumn(
                name: "Rol",
                table: "Usuarios",
                newName: "RolId");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Usuarios",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId1",
                table: "Cajas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LimiteDescuento = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PuedeAjustarInventario = table.Column<bool>(type: "bit", nullable: false),
                    PuedeAnularVentas = table.Column<bool>(type: "bit", nullable: false),
                    PuedeAplicarDescuentos = table.Column<bool>(type: "bit", nullable: false),
                    PuedeGestionarProductos = table.Column<bool>(type: "bit", nullable: false),
                    PuedeGestionarUsuarios = table.Column<bool>(type: "bit", nullable: false),
                    PuedeVerCostos = table.Column<bool>(type: "bit", nullable: false),
                    PuedeVerReportes = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 20, 16, 49, 48, 809, DateTimeKind.Local).AddTicks(3219));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 20, 16, 49, 48, 809, DateTimeKind.Local).AddTicks(3221));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 20, 16, 49, 48, 809, DateTimeKind.Local).AddTicks(3223));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 20, 16, 49, 48, 809, DateTimeKind.Local).AddTicks(3224));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "FechaCreacion",
                value: new DateTime(2026, 3, 20, 16, 49, 48, 809, DateTimeKind.Local).AddTicks(3225));

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Descripcion", "Estado", "FechaCreacion", "FechaModificacion", "LimiteDescuento", "Nombre", "PuedeAjustarInventario", "PuedeAnularVentas", "PuedeAplicarDescuentos", "PuedeGestionarProductos", "PuedeGestionarUsuarios", "PuedeVerCostos", "PuedeVerReportes" },
                values: new object[,]
                {
                    { 1, "Acceso total al sistema", 1, new DateTime(2026, 3, 20, 16, 49, 48, 813, DateTimeKind.Local).AddTicks(8704), null, 100m, "Administrador", true, true, true, true, true, true, true },
                    { 2, "Usuario de caja para ventas", 1, new DateTime(2026, 3, 20, 16, 49, 48, 813, DateTimeKind.Local).AddTicks(8707), null, 10m, "Cajero", false, false, true, false, false, false, false }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Bloqueado", "CreadoPorId", "Email", "Estado", "FechaCreacion", "FechaModificacion", "IntentosFailidos", "ModificadoPorId", "NombreCompleto", "NombreUsuario", "PasswordHash", "RolId", "Telefono", "UltimoAcceso" },
                values: new object[] { 1, false, null, "admin@possalsamentaria.com", 1, new DateTime(2026, 3, 20, 16, 49, 48, 814, DateTimeKind.Local).AddTicks(2206), null, 0, null, "Administrador del Sistema", "admin", "AQAAAAEAACcQAAAAEJ6vL8K1vxdKlVJxK3fP0LxJxHYH7sJYZMZR3rH5xJZmK4fQzH0yL9pK8mJ7vL2Q==", 1, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_UsuarioId1",
                table: "Cajas",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cajas_Usuarios_UsuarioId1",
                table: "Cajas",
                column: "UsuarioId1",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Roles_RolId",
                table: "Usuarios",
                column: "RolId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
