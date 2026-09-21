using POS.Application.DTOs.Purchases;
using POS.Application.Interfaces;
using POS.Domain.Entities.Inventory;
using POS.Domain.Entities.Purchases;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;

namespace POS.Application.Services
{
    public class CompraService : ICompraService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompraService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CompraDto> RegistrarCompraAsync(CrearCompraDto dto, int usuarioId)
        {
            if (!dto.Detalles.Any())
                throw new Exception("La compra debe tener al menos un producto.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var numeroCompra = await _unitOfWork.Compras.GenerarNumeroCompraAsync();

                var compra = new Compra
                {
                    NumeroCompra = numeroCompra,
                    NumeroFactura = dto.NumeroFactura,
                    ProveedorId = dto.ProveedorId,
                    UsuarioId = usuarioId,
                    FechaCompra = dto.FechaCompra,
                    Pagado = dto.Pagado,
                    FechaPago = dto.Pagado ? DateTime.Now : null,
                    FechaCreacion = DateTime.Now
                };

                decimal subtotal = 0;
                decimal impuestoTotal = 0;

                foreach (var item in dto.Detalles)
                {
                    var producto = await _unitOfWork.Productos.GetByIdAsync(item.ProductoId);
                    if (producto == null)
                        throw new Exception($"Producto ID {item.ProductoId} no encontrado.");

                    // El valor para la factura (Compra) es el Precio Neto + IVA
                    var valorCompraUnitario = item.PrecioSinIva - item.DescuentoValor + item.IvaValor;
                    var subtotalLinea = valorCompraUnitario * item.Cantidad;
                    
                    var costoLiquidadoLinea = item.CostoUnitarioLiquidado * item.Cantidad;
                    
                    subtotal += subtotalLinea;
                    impuestoTotal += item.IvaValor * item.Cantidad; // Solo IVA va como impuesto general de factura

                    // Crear lote si se especificó lote
                    Lote? loteObj = null;
                    if (!string.IsNullOrWhiteSpace(item.LoteCodigo))
                    {
                        loteObj = new Lote
                        {
                            ProductoId = item.ProductoId,
                            NumeroLote = item.LoteCodigo.Trim(),
                            CantidadInicial = item.Cantidad,
                            CantidadActual = item.Cantidad,
                            FechaIngreso = DateTime.Now,
                            FechaVencimiento = item.FechaVencimiento,
                            CostoUnitario = item.CostoUnitarioLiquidado,
                            Estado = EstadoRegistro.Activo
                        };
                        await _unitOfWork.Lotes.AddAsync(loteObj);
                        await _unitOfWork.SaveChangesAsync(); // Generar ID
                    }

                    compra.DetallesCompra.Add(new DetalleCompra
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        Descuento = item.Descuento,
                        Subtotal = subtotalLinea,
                        FechaVencimiento = item.FechaVencimiento,
                        PrecioSinIva = item.PrecioSinIva,
                        PrecioConIva = item.PrecioConIva,
                        IvaPorcentaje = item.IvaPorcentaje,
                        IvaValor = item.IvaValor,
                        DescuentoPorcentaje = item.DescuentoPorcentaje,
                        DescuentoValor = item.DescuentoValor,
                        IbuaPorcentaje = item.IbuaPorcentaje,
                        IbuaValor = item.IbuaValor,
                        IcuiPorcentaje = item.IcuiPorcentaje,
                        IcuiValor = item.IcuiValor,
                        CostoUnitarioLiquidado = item.CostoUnitarioLiquidado,
                        PorcentajeGanancia = item.PorcentajeGanancia,
                        PrecioVentaCalculado = item.PrecioVentaCalculado,
                        LoteCodigo = item.LoteCodigo,
                        ActualizarCatalogo = item.ActualizarCatalogo,
                        LoteId = loteObj?.Id,
                        Observaciones = item.Observaciones
                    });

                    var stockAnterior = producto.StockActual;
                    producto.StockActual += item.Cantidad;
                    // El costo de compra del producto pasa a ser el precio base neto (sin impuestos)
                    // para no acumular el IVA cada vez que se carga el producto en futuras compras
                    var baseNeto = item.PrecioSinIva - item.DescuentoValor;
                    producto.PrecioCompra = baseNeto > 0 ? baseNeto : item.PrecioSinIva;

                    if (item.ActualizarCatalogo)
                    {
                        producto.PrecioVenta = item.PrecioVentaCalculado;
                    }
                    producto.FechaModificacion = DateTime.Now;
                    _unitOfWork.Productos.Update(producto);

                    await _unitOfWork.MovimientosInventario.AddAsync(new MovimientoInventario
                    {
                        ProductoId = item.ProductoId,
                        Tipo = TipoMovimientoInventario.Entrada,
                        Cantidad = item.Cantidad,
                        StockAnterior = stockAnterior,
                        StockNuevo = producto.StockActual,
                        CostoUnitario = item.CostoUnitarioLiquidado,
                        CostoTotal = costoLiquidadoLinea,
                        Motivo = $"Compra {numeroCompra}",
                        UsuarioId = usuarioId,
                        Fecha = DateTime.Now
                    });
                }

                compra.Subtotal = subtotal;
                compra.Descuento = dto.Detalles.Sum(d => d.DescuentoValor * d.Cantidad);
                compra.Impuesto = impuestoTotal;
                compra.Total = subtotal;

                await _unitOfWork.Compras.AddAsync(compra);
                await _unitOfWork.CommitTransactionAsync();

                var guardada = await _unitOfWork.Compras.GetCompraConDetallesAsync(compra.Id);
                return guardada != null ? MapToDto(guardada) : MapToDto(compra);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<CompraDto>> ObtenerPorRangoAsync(DateTime inicio, DateTime fin)
        {
            var compras = await _unitOfWork.Compras.GetComprasPorRangoAsync(
                inicio.Date,
                fin.Date.AddDays(1).AddTicks(-1));
            return compras.Select(MapToDto);
        }

        public async Task<CompraDto?> ObtenerPorIdAsync(int id)
        {
            var compra = await _unitOfWork.Compras.GetCompraConDetallesAsync(id);
            return compra == null ? null : MapToDto(compra);
        }

        public async Task AnularCompraAsync(int id, string motivo, int usuarioId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var compra = await _unitOfWork.Compras.GetCompraConDetallesAsync(id);
                if (compra == null) throw new Exception("Compra no encontrada.");
                if (compra.Anulada) throw new Exception("La compra ya está anulada.");

                compra.Anulada = true;
                compra.FechaAnulacion = DateTime.Now;
                compra.MotivoAnulacion = motivo;
                _unitOfWork.Compras.Update(compra);

                foreach (var detalle in compra.DetallesCompra)
                {
                    var producto = await _unitOfWork.Productos.GetByIdAsync(detalle.ProductoId);
                    if (producto == null) continue;

                    var stockAnterior = producto.StockActual;
                    producto.StockActual = Math.Max(0, producto.StockActual - detalle.Cantidad);
                    producto.FechaModificacion = DateTime.Now;
                    _unitOfWork.Productos.Update(producto);

                    await _unitOfWork.MovimientosInventario.AddAsync(new MovimientoInventario
                    {
                        ProductoId = detalle.ProductoId,
                        Tipo = TipoMovimientoInventario.DevolucionProveedor,
                        Cantidad = detalle.Cantidad,
                        StockAnterior = stockAnterior,
                        StockNuevo = producto.StockActual,
                        Motivo = $"Anulación {compra.NumeroCompra}: {motivo}",
                        UsuarioId = usuarioId,
                        Fecha = DateTime.Now
                    });
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task MarcarPagadaAsync(int id)
        {
            var compra = await _unitOfWork.Compras.GetByIdAsync(id);
            if (compra == null) throw new Exception("Compra no encontrada.");
            compra.Pagado = true;
            compra.FechaPago = DateTime.Now;
            _unitOfWork.Compras.Update(compra);
            await _unitOfWork.SaveChangesAsync();
        }

        private static CompraDto MapToDto(Compra c) => new()
        {
            Id = c.Id,
            NumeroCompra = c.NumeroCompra,
            NumeroFactura = c.NumeroFactura,
            ProveedorId = c.ProveedorId,
            ProveedorNombre = c.Proveedor?.Nombre ?? string.Empty,
            UsuarioNombre = c.Usuario?.NombreCompleto ?? string.Empty,
            FechaCompra = c.FechaCompra,
            Subtotal = c.Subtotal,
            Descuento = c.Descuento,
            Impuesto = c.Impuesto,
            Total = c.Total,
            Pagado = c.Pagado,
            Anulada = c.Anulada,
            MotivoAnulacion = c.MotivoAnulacion,
            Detalles = c.DetallesCompra.Select(d => new DetalleCompraDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                ProductoCodigo = d.Producto?.Codigo ?? string.Empty,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento,
                Subtotal = d.Subtotal,
                FechaVencimiento = d.FechaVencimiento,
                PrecioSinIva = d.PrecioSinIva,
                PrecioConIva = d.PrecioConIva,
                IvaPorcentaje = d.IvaPorcentaje,
                IvaValor = d.IvaValor,
                DescuentoPorcentaje = d.DescuentoPorcentaje,
                DescuentoValor = d.DescuentoValor,
                IbuaPorcentaje = d.IbuaPorcentaje,
                IbuaValor = d.IbuaValor,
                IcuiPorcentaje = d.IcuiPorcentaje,
                IcuiValor = d.IcuiValor,
                CostoUnitarioLiquidado = d.CostoUnitarioLiquidado,
                PorcentajeGanancia = d.PorcentajeGanancia,
                PrecioVentaCalculado = d.PrecioVentaCalculado,
                LoteCodigo = d.LoteCodigo,
                ActualizarCatalogo = d.ActualizarCatalogo
            }).ToList()
        };
    }
}
