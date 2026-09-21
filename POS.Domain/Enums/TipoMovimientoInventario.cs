namespace POS.Domain.Enums
{
    public enum TipoMovimientoInventario
    {
        Entrada = 1,           // Compra
        Salida = 2,            // Venta
        Ajuste = 3,            // Corrección manual
        Merma = 4,             // Producto vencido/dañado
        DevolucionCliente = 5, // Cliente devuelve
        DevolucionProveedor = 6, // Devolución a proveedor
        AnulacionVenta = 7  // ✅ AGREGAR
    }
}