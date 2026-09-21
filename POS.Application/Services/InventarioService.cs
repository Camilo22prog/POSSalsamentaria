using POS.Application.DTOs.Inventory;
using POS.Application.Interfaces;
using POS.Domain.Entities.Inventory;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;

namespace POS.Application.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventarioService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<InventarioProductoDto>> ObtenerInventarioActualAsync()
        {
            var productos = await _unitOfWork.Productos.GetProductosActivosAsync();
            var inventario = new List<InventarioProductoDto>();

            foreach (var producto in productos)
            {
                inventario.Add(new InventarioProductoDto
                {
                    ProductoId = producto.Id,
                    Codigo = producto.Codigo,
                    Nombre = producto.Nombre,
                    CategoriaNombre = producto.Categoria?.Nombre ?? "",
                    CategoriaColor = producto.Categoria?.Color,
                    StockActual = producto.StockActual,
                    StockMinimo = producto.StockMinimo,
                    TieneStockBajo = producto.StockActual <= producto.StockMinimo,
                    UnidadMedida = producto.UnidadMedida,
                    CostoPromedio = producto.PrecioCompra,
                    ValorInventario = producto.StockActual * producto.PrecioCompra,
                    UltimoMovimiento = producto.FechaModificacion ?? producto.FechaCreacion,
                    PrecioCompra = producto.PrecioCompra,
                    PrecioVenta = producto.PrecioVenta,
                    TipoIVA = producto.TipoIVA
                });
            }

            return inventario.OrderBy(i => i.Nombre);
        }

        public async Task<IEnumerable<InventarioProductoDto>> ObtenerProductosConStockBajoAsync()
        {
            var inventario = await ObtenerInventarioActualAsync();
            return inventario.Where(i => i.TieneStockBajo);
        }

        public async Task<InventarioProductoDto?> ObtenerInventarioPorProductoAsync(int productoId)
        {
            var inventario = await ObtenerInventarioActualAsync();
            return inventario.FirstOrDefault(i => i.ProductoId == productoId);
        }

        public async Task AjustarStockAsync(AjustarStockDto dto, int usuarioId)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(dto.ProductoId);
            if (producto == null)
                throw new Exception("Producto no encontrado");

            var stockAnterior = producto.StockActual;
            var diferencia = dto.CantidadNueva - stockAnterior;

            // Actualizar stock del producto
            producto.StockActual = dto.CantidadNueva;
            
            if (dto.PrecioCompraNuevo.HasValue)
            {
                producto.PrecioCompra = dto.PrecioCompraNuevo.Value;
            }
            if (dto.PrecioVentaNuevo.HasValue)
            {
                producto.PrecioVenta = dto.PrecioVentaNuevo.Value;
            }

            producto.FechaModificacion = DateTime.Now;
            _unitOfWork.Productos.Update(producto);

            // Registrar movimiento
            var movimiento = new MovimientoInventario
            {
                ProductoId = dto.ProductoId,
                Tipo = dto.Tipo,
                Cantidad = Math.Abs(diferencia),
                StockAnterior = stockAnterior,
                StockNuevo = dto.CantidadNueva,
                CostoUnitario = dto.CostoUnitario,
                CostoTotal = dto.CostoUnitario.HasValue ? dto.CostoUnitario.Value * Math.Abs(diferencia) : null,
                Referencia = "AJUSTE_MANUAL",
                Motivo = dto.Motivo,
                UsuarioId = usuarioId,
                Fecha = DateTime.Now
            };

            await _unitOfWork.MovimientosInventario.AddAsync(movimiento);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CargarStockInicialAsync(int productoId, decimal cantidad, decimal costoUnitario, int usuarioId)
        {
            var ajuste = new AjustarStockDto
            {
                ProductoId = productoId,
                CantidadNueva = cantidad,
                Tipo = TipoMovimientoInventario.Entrada,
                Motivo = "Carga de stock inicial",
                CostoUnitario = costoUnitario
            };

            await AjustarStockAsync(ajuste, usuarioId);
        }

        public async Task<IEnumerable<MovimientoInventarioDto>> ObtenerKardexPorProductoAsync(int productoId, DateTime? desde = null, DateTime? hasta = null)
        {
            var movimientos = await _unitOfWork.MovimientosInventario.FindAsync(m => m.ProductoId == productoId);

            var movimientosFiltrados = movimientos.AsEnumerable();

            if (desde.HasValue)
                movimientosFiltrados = movimientosFiltrados.Where(m => m.Fecha >= desde.Value);

            if (hasta.HasValue)
                movimientosFiltrados = movimientosFiltrados.Where(m => m.Fecha <= hasta.Value);

            var producto = await _unitOfWork.Productos.GetByIdAsync(productoId);
            
            var resultado = new List<MovimientoInventarioDto>();

            foreach (var movimiento in movimientosFiltrados.OrderByDescending(m => m.Fecha))
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(movimiento.UsuarioId);
                resultado.Add(MapToDto(
                    movimiento,
                    producto?.Nombre ?? "",
                    producto?.Codigo ?? "",
                    usuario?.NombreCompleto ?? ""
                ));
            }

            return resultado;
        }

        public async Task<IEnumerable<MovimientoInventarioDto>> ObtenerMovimientosRecientesAsync(int cantidad = 20)
        {
            var todosMovimientos = await _unitOfWork.MovimientosInventario.GetAllAsync();
            
            var movimientosOrdenados = todosMovimientos
                .OrderByDescending(m => m.Fecha)
                .Take(cantidad)
                .ToList();

            var resultado = new List<MovimientoInventarioDto>();

            foreach (var movimiento in movimientosOrdenados)
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(movimiento.ProductoId);
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(movimiento.UsuarioId);

                resultado.Add(MapToDto(
                    movimiento,
                    producto?.Nombre ?? "",
                    producto?.Codigo ?? "",
                    usuario?.NombreCompleto ?? ""
                ));
            }

            return resultado;
        }

        public async Task<decimal> ObtenerValorTotalInventarioAsync()
        {
            var inventario = await ObtenerInventarioActualAsync();
            return inventario.Sum(i => i.ValorInventario ?? 0);
        }

        public async Task<int> ContarProductosConStockAsync()
        {
            var inventario = await ObtenerInventarioActualAsync();
            return inventario.Count(i => i.StockActual > 0);
        }

        public async Task<int> ContarProductosConStockBajoAsync()
        {
            var inventario = await ObtenerInventarioActualAsync();
            return inventario.Count(i => i.TieneStockBajo);
        }

        private MovimientoInventarioDto MapToDto(MovimientoInventario movimiento, string productoNombre, string productoCodigo, string usuarioNombre)
        {
            return new MovimientoInventarioDto
            {
                Id = movimiento.Id,
                ProductoId = movimiento.ProductoId,
                ProductoNombre = productoNombre,
                ProductoCodigo = productoCodigo,
                Tipo = movimiento.Tipo,
                TipoTexto = ObtenerTextoTipoMovimiento(movimiento.Tipo),
                Cantidad = movimiento.Cantidad,
                StockAnterior = movimiento.StockAnterior,
                StockNuevo = movimiento.StockNuevo,
                CostoUnitario = movimiento.CostoUnitario,
                CostoTotal = movimiento.CostoTotal,
                Referencia = movimiento.Referencia,
                Motivo = movimiento.Motivo,
                UsuarioId = movimiento.UsuarioId,
                UsuarioNombre = usuarioNombre,
                Fecha = movimiento.Fecha
            };
        }

        private string ObtenerTextoTipoMovimiento(TipoMovimientoInventario tipo)
        {
            return tipo switch
            {
                TipoMovimientoInventario.Entrada => "Entrada",
                TipoMovimientoInventario.Salida => "Salida",
                TipoMovimientoInventario.Ajuste => "Ajuste",
                TipoMovimientoInventario.Merma => "Merma",
                TipoMovimientoInventario.DevolucionCliente => "Devolución Cliente",
                TipoMovimientoInventario.DevolucionProveedor => "Devolución Proveedor",
                _ => ""
            };
        }
    }
}