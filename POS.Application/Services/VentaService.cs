using POS.Application.DTOs.Sales;
using POS.Application.Interfaces;
using POS.Domain.Entities.Sales;
using POS.Domain.Entities.Inventory;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;

namespace POS.Application.Services
{
    public class VentaService : IVentaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VentaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VentaDto> CrearVentaAsync(CrearVentaDto dto, int usuarioId, int cajaId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Validar que haya items
                if (!dto.Items.Any())
                    throw new Exception("La venta debe tener al menos un producto");

                // Validar que haya pagos
                if (!dto.Pagos.Any())
                    throw new Exception("La venta debe tener al menos un método de pago");

                // Generar número de venta
                var numeroVenta = await _unitOfWork.Ventas.GenerarNumeroVentaAsync();

                // Crear venta
                var venta = new Venta
                {
                    NumeroVenta = numeroVenta,
                    Fecha = DateTime.Now,
                    ClienteId = dto.ClienteId,
                    UsuarioId = usuarioId,
                    CajaId = cajaId,
                    Anulada = false
                };

                // Procesar items
                decimal subtotal = 0;
                decimal ivaTotal = 0;
                decimal descuentoTotal = 0;

                foreach (var item in dto.Items)
                {
                    var producto = await _unitOfWork.Productos.GetByIdAsync(item.ProductoId);
                    if (producto == null)
                        throw new Exception($"Producto con ID {item.ProductoId} no encontrado");

                    // Calcular precios (PRECIO YA INCLUYE IVA)
                    var precioUnitario = producto.VentaPorPeso ? producto.PrecioPorKilo : producto.PrecioVenta;
                    var descuentoLinea = item.DescuentoLinea ?? 0;
                    var subtotalLinea = Math.Round((precioUnitario * item.Cantidad) - descuentoLinea, 0, MidpointRounding.AwayFromZero);
                    
                    // Calcular IVA INCLUIDO en el precio
                    var porcentajeIVA = (decimal)producto.TipoIVA / 100;
                    decimal montoIVA = 0;
                    
                    if (porcentajeIVA > 0)
                    {
                        var factorIVA = 1 + porcentajeIVA;
                        montoIVA = subtotalLinea - (subtotalLinea / factorIVA);
                    }
                    
                    var totalLinea = subtotalLinea; // El total ES el subtotal (IVA incluido)

                    // Crear detalle
                    var detalle = new DetalleVenta
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = precioUnitario,
                        Descuento = descuentoLinea,
                        PorcentajeIVA = porcentajeIVA,
                        MontoIVA = montoIVA,
                        Subtotal = subtotalLinea,
                        Total = totalLinea,
                        CostoUnitario = producto.PrecioCompra
                    };

                    venta.Detalles.Add(detalle);

                    subtotal += subtotalLinea;
                    ivaTotal += montoIVA;
                    descuentoTotal += descuentoLinea;

                    // Descontar stock (permitir negativos)
                    var stockAnterior = producto.StockActual;
                    producto.StockActual -= item.Cantidad;
                    _unitOfWork.Productos.Update(producto);

                    // Registrar movimiento de inventario
                    var movimiento = new MovimientoInventario
                    {
                        ProductoId = item.ProductoId,
                        Tipo = TipoMovimientoInventario.Salida,
                        Cantidad = item.Cantidad,
                        StockAnterior = stockAnterior,
                        StockNuevo = producto.StockActual,
                        CostoUnitario = producto.PrecioCompra,
                        CostoTotal = producto.PrecioCompra * item.Cantidad,
                        Referencia = numeroVenta,
                        Motivo = "Venta",
                        UsuarioId = usuarioId,
                        Fecha = DateTime.Now
                    };

                    await _unitOfWork.MovimientosInventario.AddAsync(movimiento);
                }

                // Aplicar descuento general si existe
                if (dto.DescuentoGeneral.HasValue && dto.DescuentoGeneral.Value > 0)
                {
                    descuentoTotal += dto.DescuentoGeneral.Value;
                    subtotal -= dto.DescuentoGeneral.Value;
                    
                    // Recalcular IVA después del descuento
                    ivaTotal = 0;
                    foreach (var detalle in venta.Detalles)
                    {
                        var porcentajeIVA = detalle.PorcentajeIVA;
                        if (porcentajeIVA > 0)
                        {
                            var factorIVA = 1 + porcentajeIVA;
                            var subtotalDetalle = detalle.Subtotal - (dto.DescuentoGeneral.Value / venta.Detalles.Count);
                            ivaTotal += subtotalDetalle - (subtotalDetalle / factorIVA);
                        }
                    }
                }

                // Asignar totales - EL TOTAL ES EL SUBTOTAL (IVA YA INCLUIDO)
                venta.Subtotal = subtotal;
                venta.DescuentoTotal = descuentoTotal;
                venta.IVATotal = ivaTotal;
                venta.Total = subtotal; // ← El total ES el subtotal porque IVA ya está incluido

                // Validar pagos
                var totalPagado = dto.Pagos.Sum(p => p.Monto);

                // Si el total pagado es MENOR al total, es un error
                if (totalPagado < venta.Total)
                {
                    throw new Exception($"El total de pagos ({totalPagado:C}) es menor al total de la venta ({venta.Total:C})");
                }

                // Registrar pagos
                var cambioTotal = totalPagado > venta.Total ? totalPagado - venta.Total : 0;
                var cambioAsignado = false;

                foreach (var pagoDto in dto.Pagos)
                {
                    var pago = new Pago
                    {
                        Metodo = pagoDto.Metodo,
                        Monto = pagoDto.Monto,
                        Referencia = pagoDto.Referencia
                    };

                    // Asignar cambio al primer pago procesado si hay cambio
                    if (cambioTotal > 0 && !cambioAsignado)
                    {
                        pago.Cambio = cambioTotal;
                        cambioAsignado = true;
                    }

                    venta.Pagos.Add(pago);
                }

                // Guardar venta
                await _unitOfWork.Ventas.AddAsync(venta);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Retornar DTO
                var ventaCompleta = await _unitOfWork.Ventas.GetVentaCompletaAsync(venta.Id);
                return MapToDto(ventaCompleta!);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<VentaDto?> ObtenerVentaPorIdAsync(int ventaId)
        {
            var venta = await _unitOfWork.Ventas.GetVentaCompletaAsync(ventaId);
            return venta != null ? MapToDto(venta) : null;
        }

        public async Task<IEnumerable<VentaDto>> ObtenerVentasPorCajaAsync(int cajaId)
        {
            var ventas = await _unitOfWork.Ventas.GetVentasPorCajaAsync(cajaId);
            return ventas.Select(MapToDto);
        }

        public async Task<IEnumerable<VentaDto>> ObtenerVentasDelDiaAsync()
        {
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaAsync(DateTime.Now);
            return ventas.Select(MapToDto);
        }

        public async Task<IEnumerable<VentaDto>> ObtenerVentasPorRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var fin = fechaFin.Date.AddDays(1).AddTicks(-1);
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(fechaInicio.Date, fin);
            return ventas.Select(MapToDto);
        }

        // ✅ MÉTODO MEJORADO PARA ANULAR VENTAS
        public async Task<bool> AnularVentaAsync(int ventaId, string motivo, int usuarioId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var venta = await _unitOfWork.Ventas.GetVentaCompletaAsync(ventaId);
                if (venta == null)
                    throw new Exception("Venta no encontrada");

                if (venta.Anulada)
                    throw new Exception("Esta venta ya fue anulada previamente");

                // Validar que la venta sea del día actual (OPCIONAL - comenta si quieres anular ventas viejas)
                if (venta.Fecha.Date != DateTime.Now.Date)
                    throw new Exception("Solo se pueden anular ventas del día actual");

                // Marcar como anulada
                venta.Anulada = true;
                venta.MotivoAnulacion = motivo;
                venta.FechaAnulacion = DateTime.Now;
                venta.AnuladaPorUsuarioId = usuarioId;
                
                _unitOfWork.Ventas.Update(venta);

                // Devolver stock y registrar movimientos
                foreach (var detalle in venta.Detalles)
                {
                    var producto = await _unitOfWork.Productos.GetByIdAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        var stockAnterior = producto.StockActual;
                        producto.StockActual += (int)detalle.Cantidad;
                        _unitOfWork.Productos.Update(producto);

                        // Registrar movimiento de devolución
                        var movimiento = new MovimientoInventario
                        {
                            ProductoId = detalle.ProductoId,
                            Tipo = TipoMovimientoInventario.DevolucionCliente,
                            Cantidad = (int)detalle.Cantidad,
                            StockAnterior = stockAnterior,
                            StockNuevo = producto.StockActual,
                            CostoUnitario = detalle.CostoUnitario,
                            CostoTotal = detalle.CostoUnitario * (int)detalle.Cantidad,
                            Referencia = venta.NumeroVenta,
                            Motivo = $"Anulación de venta: {motivo}",
                            UsuarioId = usuarioId,
                            Fecha = DateTime.Now
                        };

                        await _unitOfWork.MovimientosInventario.AddAsync(movimiento);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                return true;  // ✅ AGREGAR ESTE RETURN
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<decimal> ObtenerTotalVentasDelDiaAsync()
        {
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaAsync(DateTime.Now);
            return ventas.Where(v => !v.Anulada).Sum(v => v.Total);  // ✅ Excluir anuladas
        }

        public async Task<int> ContarVentasDelDiaAsync()
        {
            return await _unitOfWork.Ventas.ContarVentasDelDiaAsync();
        }

        private VentaDto MapToDto(Venta venta)
        {
            return new VentaDto
            {
                Id = venta.Id,
                NumeroVenta = venta.NumeroVenta,
                Fecha = venta.Fecha,
                ClienteId = venta.ClienteId,
                ClienteNombre = venta.Cliente?.NombreCompleto ?? "Consumidor Final",
                UsuarioId = venta.UsuarioId,
                UsuarioNombre = venta.Usuario?.NombreCompleto ?? "",
                CajaId = venta.CajaId,
                Subtotal = venta.Subtotal,
                DescuentoTotal = venta.DescuentoTotal,
                IVATotal = venta.IVATotal,
                Total = venta.Total,
                Anulada = venta.Anulada,
                MotivoAnulacion = venta.MotivoAnulacion,
                FechaAnulacion = venta.FechaAnulacion,  // ✅ AGREGAR
                Detalles = venta.Detalles.Select(d => new DetalleVentaDto
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    ProductoCodigo = d.Producto?.Codigo ?? "",
                    ProductoNombre = d.Producto?.Nombre ?? "",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Descuento = d.Descuento,
                    PorcentajeIVA = d.PorcentajeIVA,
                    MontoIVA = d.MontoIVA,
                    Subtotal = d.Subtotal,
                    Total = d.Total
                }).ToList(),
                Pagos = venta.Pagos.Select(p => new PagoDto
                {
                    Id = p.Id,
                    VentaId = p.VentaId,
                    Metodo = p.Metodo,
                    MetodoTexto = ObtenerTextoMetodoPago(p.Metodo),
                    Monto = p.Monto,
                    Cambio = p.Cambio,
                    Referencia = p.Referencia
                }).ToList()
            };
        }

        private string ObtenerTextoMetodoPago(MetodoPago metodo)
        {
            return metodo switch
            {
                MetodoPago.Efectivo => "Efectivo",
                MetodoPago.Tarjeta => "Tarjeta",
                MetodoPago.Transferencia => "Transferencia",
                MetodoPago.QR => "QR",
                MetodoPago.Credito => "Crédito",
                MetodoPago.Nequi => "Nequi",
                MetodoPago.Daviplata => "Daviplata",
                _ => ""
            };
        }
    }
}