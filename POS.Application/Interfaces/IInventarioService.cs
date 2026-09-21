using POS.Application.DTOs.Inventory;

namespace POS.Application.Interfaces
{
    public interface IInventarioService
    {
        // Inventario actual
        Task<IEnumerable<InventarioProductoDto>> ObtenerInventarioActualAsync();
        Task<IEnumerable<InventarioProductoDto>> ObtenerProductosConStockBajoAsync();
        Task<InventarioProductoDto?> ObtenerInventarioPorProductoAsync(int productoId);
        
        // Ajustes de stock
        Task AjustarStockAsync(AjustarStockDto dto, int usuarioId);
        Task CargarStockInicialAsync(int productoId, decimal cantidad, decimal costoUnitario, int usuarioId);
        
        // Kardex
        Task<IEnumerable<MovimientoInventarioDto>> ObtenerKardexPorProductoAsync(int productoId, DateTime? desde = null, DateTime? hasta = null);
        Task<IEnumerable<MovimientoInventarioDto>> ObtenerMovimientosRecientesAsync(int cantidad = 20);
        
        // Estadísticas
        Task<decimal> ObtenerValorTotalInventarioAsync();
        Task<int> ContarProductosConStockAsync();
        Task<int> ContarProductosConStockBajoAsync();
    }
}