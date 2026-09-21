using ClosedXML.Excel;
using POS.Application.DTOs.Reports;
using POS.Application.Interfaces;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;
using System.Data;
using POS.Application.DTOs.Dashboard;


namespace POS.Application.Services
{
    public class ReporteService : IReporteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReporteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region VENTAS POR PERÍODO

        public async Task<ResumenVentasPorPeriodoDto> ObtenerVentasPorPeriodoAsync(FiltroReporteDto filtro)
        {
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(
                filtro.FechaInicio, 
                filtro.FechaFin);

            // Filtrar solo ventas no anuladas
            ventas = ventas.Where(v => !v.Anulada).ToList();

            // Aplicar filtros adicionales
            if (filtro.UsuarioId.HasValue)
                ventas = ventas.Where(v => v.UsuarioId == filtro.UsuarioId.Value).ToList();

            if (filtro.ClienteId.HasValue)
                ventas = ventas.Where(v => v.ClienteId == filtro.ClienteId.Value).ToList();

            // Agrupar por día
            var ventasPorDia = ventas
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new VentasPorPeriodoDto
                {
                    Fecha = g.Key,
                    CantidadVentas = g.Count(),
                    TotalVentas = g.Sum(v => v.Total),
                    TotalCosto = g.Sum(v => v.Detalles.Sum(d => d.CostoUnitario * d.Cantidad)),
                    TicketPromedio = g.Average(v => v.Total),
                    UtilidadBruta = g.Sum(v => v.Total) - g.Sum(v => v.Detalles.Sum(d => d.CostoUnitario * d.Cantidad)),
                    MargenPorcentaje = g.Sum(v => v.Total) > 0 
                        ? ((g.Sum(v => v.Total) - g.Sum(v => v.Detalles.Sum(d => d.CostoUnitario * d.Cantidad))) / g.Sum(v => v.Total)) * 100 
                        : 0
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            // Calcular totales
            var totalVentas = ventas.Count();
            var totalIngresos = ventas.Sum(v => v.Total);
            var totalCosto = ventas.Sum(v => v.Detalles.Sum(d => d.CostoUnitario * d.Cantidad));
            var utilidadBruta = totalIngresos - totalCosto;
            var margenPorcentaje = totalIngresos > 0 ? (utilidadBruta / totalIngresos) * 100 : 0;

            // Calcular crecimiento vs período anterior
            var diasDiferencia = (filtro.FechaFin - filtro.FechaInicio).Days + 1;
            var fechaInicioPeriodoAnterior = filtro.FechaInicio.AddDays(-diasDiferencia);
            var fechaFinPeriodoAnterior = filtro.FechaInicio.AddDays(-1);

            var ventasPeriodoAnterior = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(
                fechaInicioPeriodoAnterior, 
                fechaFinPeriodoAnterior);
            ventasPeriodoAnterior = ventasPeriodoAnterior.Where(v => !v.Anulada).ToList();

            var totalPeriodoAnterior = ventasPeriodoAnterior.Sum(v => v.Total);
            var crecimiento = totalPeriodoAnterior > 0 
                ? ((totalIngresos - totalPeriodoAnterior) / totalPeriodoAnterior) * 100 
                : 0;

            return new ResumenVentasPorPeriodoDto
            {
                FechaInicio = filtro.FechaInicio,
                FechaFin = filtro.FechaFin,
                TotalVentas = totalVentas,
                TotalIngresos = totalIngresos,
                TicketPromedio = totalVentas > 0 ? totalIngresos / totalVentas : 0,
                TotalCosto = totalCosto,
                UtilidadBruta = utilidadBruta,
                MargenPorcentaje = margenPorcentaje,
                CrecimientoVsPeriodoAnterior = crecimiento,
                DetallesPorDia = ventasPorDia
            };
        }

        #endregion

        #region VENTAS POR PRODUCTO

        public async Task<List<VentasPorProductoDto>> ObtenerVentasPorProductoAsync(FiltroReporteDto filtro)
        {
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(
                filtro.FechaInicio, 
                filtro.FechaFin);

            ventas = ventas.Where(v => !v.Anulada).ToList();

            // Extraer todos los detalles de venta
            var detalles = ventas.SelectMany(v => v.Detalles).ToList();

            // Aplicar filtro de categoría si existe
            if (filtro.CategoriaId.HasValue)
            {
                detalles = detalles.Where(d => d.Producto?.CategoriaId == filtro.CategoriaId.Value).ToList();
            }

            // Agrupar por producto
            var ventasPorProducto = detalles
                .GroupBy(d => new { d.ProductoId, d.Producto?.Codigo, d.Producto?.Nombre, CategoriaNombre = d.Producto?.Categoria?.Nombre })
                .Select(g => new VentasPorProductoDto
                {
                    ProductoId = g.Key.ProductoId,
                    Codigo = g.Key.Codigo ?? "",
                    Nombre = g.Key.Nombre ?? "",
                    Categoria = g.Key.CategoriaNombre ?? "",
                    CantidadVendida = (int)g.Sum(d => d.Cantidad),
                    PrecioPromedio = g.Average(d => d.PrecioUnitario),
                    TotalVentas = g.Sum(d => d.Total),
                    CostoTotal = g.Sum(d => d.CostoUnitario * d.Cantidad),
                    UtilidadBruta = g.Sum(d => d.Total) - g.Sum(d => d.CostoUnitario * d.Cantidad),
                    MargenPorcentaje = g.Sum(d => d.Total) > 0 
                        ? ((g.Sum(d => d.Total) - g.Sum(d => d.CostoUnitario * d.Cantidad)) / g.Sum(d => d.Total)) * 100 
                        : 0
                })
                .OrderByDescending(x => x.TotalVentas)
                .ToList();

            // Calcular porcentaje del total
            var totalGeneral = ventasPorProducto.Sum(x => x.TotalVentas);
            foreach (var item in ventasPorProducto)
            {
                item.PorcentajeDelTotal = totalGeneral > 0 ? (item.TotalVentas / totalGeneral) * 100 : 0;
            }

            return ventasPorProducto;
        }

        #endregion

        #region DESEMPEÑO DE USUARIOS
        public async Task<ResumenDesempenoDto> ObtenerDesempenoUsuariosAsync(FiltroReporteDto filtro)
        {
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(
                filtro.FechaInicio.Date,
                filtro.FechaFin.Date
            );

            var todasLasVentas = ventas.ToList();
            var ventasActivas = todasLasVentas.Where(v => !v.Anulada).ToList();

            // Agrupar por usuario
            var ventasPorUsuario = todasLasVentas
                .GroupBy(v => v.UsuarioId)
                .Select(g => new
                {
                    UsuarioId = g.Key,
                    Usuario = g.First().Usuario,
                    VentasActivas = g.Where(v => !v.Anulada).ToList(),
                    VentasAnuladas = g.Where(v => v.Anulada).ToList()
                })
                .ToList();

            var totalIngresos = ventasActivas.Sum(v => v.Total);
            var diasPeriodo = (filtro.FechaFin.Date - filtro.FechaInicio.Date).Days + 1;

            var usuarios = new List<DesempenoUsuarioDto>();

            foreach (var item in ventasPorUsuario)
            {
                var cantidadVentas = item.VentasActivas.Count;
                var totalVentas = item.VentasActivas.Sum(v => v.Total);
                var ventasAnuladas = item.VentasAnuladas.Count;
                var totalVentasConAnuladas = cantidadVentas + ventasAnuladas;

                var descuentos = item.VentasActivas.Sum(v => v.DescuentoTotal);
                var porcentajeDescuentos = totalVentas > 0 ? (descuentos / totalVentas) * 100 : 0;

                // Calcular ventas por método de pago
                var totalMontoEfectivo = item.VentasActivas
                    .SelectMany(v => v.Pagos)
                    .Where(p => p.Metodo == MetodoPago.Efectivo)
                    .Sum(p => p.Monto);
                
                var totalCambioEntregado = item.VentasActivas
                    .Sum(v => v.Pagos.Sum(p => p.Cambio ?? 0));

                var ventasEfectivo = totalMontoEfectivo - totalCambioEntregado;

                var ventasTarjeta = item.VentasActivas
                    .SelectMany(v => v.Pagos)
                    .Where(p => p.Metodo == MetodoPago.Tarjeta)
                    .Sum(p => p.Monto);

                var ventasTransferencia = item.VentasActivas
                    .SelectMany(v => v.Pagos)
                    .Where(p => p.Metodo == MetodoPago.Transferencia)
                    .Sum(p => p.Monto);

                var ventasNequi = item.VentasActivas
                    .SelectMany(v => v.Pagos)
                    .Where(p => p.Metodo == MetodoPago.Nequi)
                    .Sum(p => p.Monto);

                var ventasDaviplata = item.VentasActivas
                    .SelectMany(v => v.Pagos)
                    .Where(p => p.Metodo == MetodoPago.Daviplata)
                    .Sum(p => p.Monto);

                var ventasQR = item.VentasActivas
                    .SelectMany(v => v.Pagos)
                    .Where(p => p.Metodo == MetodoPago.QR)
                    .Sum(p => p.Monto);

                // Días activo (días únicos con al menos una venta)
                var diasActivo = item.VentasActivas
                    .Select(v => v.Fecha.Date)
                    .Distinct()
                    .Count();

                usuarios.Add(new DesempenoUsuarioDto
                {
                    UsuarioId = item.UsuarioId,
                    NombreUsuario = item.Usuario?.NombreUsuario ?? "Usuario",
                    NombreCompleto = item.Usuario?.NombreCompleto ?? "Usuario eliminado",
                    Rol = item.Usuario != null ? ObtenerNombreRol(item.Usuario.Rol) : "Sin rol",
                    CantidadVentas = cantidadVentas,
                    TotalVentas = totalVentas,
                    PorcentajeVentas = totalIngresos > 0 ? (totalVentas / totalIngresos) * 100 : 0,
                    TicketPromedio = cantidadVentas > 0 ? totalVentas / cantidadVentas : 0,
                    VentasAnuladas = ventasAnuladas,
                    PorcentajeAnulacion = totalVentasConAnuladas > 0 
                        ? ((decimal)ventasAnuladas / totalVentasConAnuladas) * 100 
                        : 0,
                    DescuentosAplicados = descuentos,
                    PorcentajeDescuentos = porcentajeDescuentos,
                    UltimaVenta = item.VentasActivas.Any() 
                        ? item.VentasActivas.Max(v => v.Fecha) 
                        : (DateTime?)null,
                    DiasActivo = diasActivo,
                    VentasPorDia = diasActivo > 0 ? (decimal)cantidadVentas / diasActivo : 0,
                    VentasEfectivo = ventasEfectivo,
                    VentasTarjeta = ventasTarjeta,
                    VentasTransferencia = ventasTransferencia,
                    VentasNequi = ventasNequi,
                    VentasDaviplata = ventasDaviplata,
                    VentasQR = ventasQR,
                    HorasActivo = diasActivo * 8, // Estimado: 8 horas por día
                    VentasPorHora = diasActivo > 0 ? (decimal)cantidadVentas / (diasActivo * 8) : 0
                });
            }

            

            usuarios = usuarios.OrderByDescending(u => u.TotalVentas).ToList();

            var mejorVendedor = usuarios.FirstOrDefault()?.NombreCompleto ?? "N/A";

            return new ResumenDesempenoDto
            {
                TotalUsuarios = usuarios.Count,
                TotalVentas = ventasActivas.Count,
                TotalIngresos = totalIngresos,
                TicketPromedio = ventasActivas.Any() ? totalIngresos / ventasActivas.Count : 0,
                MejorVendedor = mejorVendedor,
                Usuarios = usuarios
            };

            
        }


        
            
        #endregion

        #region VENTAS POR CLIENTE

        public async Task<List<VentasPorClienteDto>> ObtenerVentasPorClienteAsync(FiltroReporteDto filtro)
        {
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(
                filtro.FechaInicio, 
                filtro.FechaFin);

            ventas = ventas.Where(v => !v.Anulada).ToList();

            var ventasPorCliente = ventas
                .GroupBy(v => new 
                { 
                    v.ClienteId, 
                    ClienteNombre = v.Cliente != null ? v.Cliente.NombreCompleto : "Consumidor Final",
                    NumeroDocumento = v.Cliente?.NumeroDocumento ?? ""
                })
                .Select(g => new VentasPorClienteDto
                {
                    ClienteId = g.Key.ClienteId,
                    ClienteNombre = g.Key.ClienteNombre,
                    NumeroDocumento = g.Key.NumeroDocumento,
                    CantidadCompras = g.Count(),
                    TotalCompras = g.Sum(v => v.Total),
                    TicketPromedio = g.Average(v => v.Total),
                    UltimaCompra = g.Max(v => v.Fecha),
                    PrimeraCompra = g.Min(v => v.Fecha),
                    DiasDesdeUltimaCompra = (DateTime.Now - g.Max(v => v.Fecha)).Days
                })
                .OrderByDescending(x => x.TotalCompras)
                .ToList();

            return ventasPorCliente;
        }

        #endregion

        #region VENTAS POR MÉTODO DE PAGO

        public async Task<List<VentasPorMetodoPagoDto>> ObtenerVentasPorMetodoPagoAsync(FiltroReporteDto filtro)
        {
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(
                filtro.FechaInicio.Date,
                filtro.FechaFin.Date
            );

            var ventasNoAnuladas = ventas.Where(v => !v.Anulada).ToList();

            if (!ventasNoAnuladas.Any())
            {
                return new List<VentasPorMetodoPagoDto>();
            }

            // Obtener todos los pagos agrupados por método
            var pagosPorMetodo = ventasNoAnuladas
                .SelectMany(v => v.Pagos.Select(p => new
                {
                    Venta = v,
                    Pago = p
                }))
                .GroupBy(x => x.Pago.Metodo)
                .Select(g => new VentasPorMetodoPagoDto
                {
                    Metodo = g.Key,
                    MetodoTexto = ObtenerNombreMetodoPago(g.Key),
                    CantidadTransacciones = g.Count(),
                    MontoTotal = g.Sum(x => x.Pago.Monto),
                    PorcentajeDelTotal = 0,
                    Detalles = g.Select(x => new DetallePagoDto
                    {
                        VentaId = x.Venta.Id,
                        NumeroVenta = x.Venta.NumeroVenta,
                        FechaVenta = x.Venta.Fecha,
                        Cliente = x.Venta.Cliente?.NombreCompleto ?? "Cliente General",
                        Monto = x.Pago.Monto,
                        NumeroAutorizacion = x.Pago.NumeroAutorizacion,
                        Referencia = x.Pago.Referencia,
                        Usuario = x.Venta.Usuario?.NombreCompleto ?? "Usuario"
                    })
                    .OrderByDescending(d => d.FechaVenta)
                    .ToList()
                })
                .OrderByDescending(m => m.MontoTotal)
                .ToList();

            // Calcular porcentajes
            var totalGeneral = pagosPorMetodo.Sum(m => m.MontoTotal);
            foreach (var metodo in pagosPorMetodo)
            {
                metodo.PorcentajeDelTotal = totalGeneral > 0
                    ? (metodo.MontoTotal / totalGeneral) * 100
                    : 0;
            }

            return pagosPorMetodo;
        }

        public async Task<ResumenVentasAnuladasDto> ObtenerVentasAnuladasAsync(FiltroReporteDto filtro)
        {
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(
                filtro.FechaInicio.Date,
                filtro.FechaFin.Date
            );

            var todasLasVentas = ventas.ToList();
            var ventasAnuladas = todasLasVentas.Where(v => v.Anulada).ToList();
            var ventasActivas = todasLasVentas.Where(v => !v.Anulada).ToList();

            // Agrupar por usuario para encontrar quien tiene más anulaciones
            var anulacionesPorUsuario = ventasAnuladas
                .GroupBy(v => v.UsuarioId)
                .Select(g => new
                {
                    UsuarioId = g.Key,
                    Usuario = g.First().Usuario?.NombreCompleto ?? "Usuario",
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .FirstOrDefault();

            var ventasAnuladasDto = ventasAnuladas.Select(v =>
            {
                var tiempoHastaAnulacion = v.FechaAnulacion.HasValue
                    ? (int)(v.FechaAnulacion.Value - v.Fecha).TotalMinutes
                    : 0;

                var metodoPago = v.Pagos?.FirstOrDefault()?.Metodo.ToString() ?? "N/A";
                if (v.Pagos != null && v.Pagos.Count > 1)
                {
                    metodoPago = "Mixto";
                }

                return new VentaAnuladaDto
                {
                    VentaId = v.Id,
                    NumeroVenta = v.NumeroVenta,
                    FechaVenta = v.Fecha,
                    FechaAnulacion = v.FechaAnulacion ?? v.Fecha,
                    Cliente = v.Cliente?.NombreCompleto ?? "Cliente General",
                    Usuario = v.Usuario?.NombreCompleto ?? "Usuario",
                    Total = v.Total,
                    MotivoAnulacion = v.MotivoAnulacion ?? "Sin motivo especificado",
                    TiempoHastaAnulacion = tiempoHastaAnulacion,
                    CantidadProductos = v.Detalles?.Sum(d => (int)d.Cantidad) ?? 0,
                    MetodoPago = metodoPago
                };
            })
            .OrderByDescending(v => v.FechaAnulacion)
            .ToList();

            var totalVentas = todasLasVentas.Count;
            var montoTotalVentas = ventasActivas.Sum(v => v.Total);
            var montoTotalAnulado = ventasAnuladas.Sum(v => v.Total);

            return new ResumenVentasAnuladasDto
            {
                TotalVentasAnuladas = ventasAnuladas.Count,
                MontoTotalAnulado = montoTotalAnulado,
                PorcentajeAnulacion = totalVentas > 0 
                    ? ((decimal)ventasAnuladas.Count / totalVentas) * 100 
                    : 0,
                VentasTotales = totalVentas,
                MontoTotalVentas = montoTotalVentas + montoTotalAnulado,
                UsuarioConMasAnulaciones = anulacionesPorUsuario?.Usuario ?? "N/A",
                CantidadMasAnulaciones = anulacionesPorUsuario?.Cantidad ?? 0,
                Ventas = ventasAnuladasDto
            };
        }

        #endregion

        #region ESTADO DE RESULTADOS

        public async Task<EstadoResultadosDto> ObtenerEstadoResultadosAsync(FiltroReporteDto filtro)
        {
            var fechaInicio = filtro.FechaInicio.Date;
            var fechaFin = filtro.FechaFin.Date.AddDays(1).AddTicks(-1);

            // ── INGRESOS ─────────────────────────────────────────────────────────
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(fechaInicio, fechaFin);

            var ventasBrutas  = ventas.Where(v => !v.Anulada).Sum(v => v.Total);
            var devoluciones  = ventas.Where(v =>  v.Anulada).Sum(v => v.Total);
            var descuentos    = ventas.Where(v => !v.Anulada).Sum(v => v.DescuentoTotal);
            // Fórmula correcta: las devoluciones Y los descuentos reducen el ingreso neto
            var ventasNetas   = ventasBrutas - devoluciones - descuentos;

            // ── COSTO DE VENTAS (inventario perpetuo) ────────────────────────────
            // El sistema registra el costo en cada DetalleVenta; se usa el costo directo
            var costoProductosVendidos = ventas
                .Where(v => !v.Anulada)
                .Sum(v => v.Detalles.Sum(d => d.CostoUnitario * d.Cantidad));

            // Las mermas son inventario que se perdió sin generar ingresos → van al costo
            var mermas = await _unitOfWork.Mermas.FindAsync(
                m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin);
            var mermasDelPeriodo = mermas.Sum(m => m.CostoTotal);

            var totalCostoVentas = costoProductosVendidos + mermasDelPeriodo;

            // ── UTILIDAD BRUTA ────────────────────────────────────────────────────
            var utilidadBruta = ventasNetas - totalCostoVentas;
            var margenBruto   = ventasNetas > 0 ? Math.Round((utilidadBruta / ventasNetas) * 100, 2) : 0;

            // ── GASTOS OPERATIVOS ─────────────────────────────────────────────────
            var gastosLista = await _unitOfWork.Gastos.FindAsync(
                g => g.Fecha >= fechaInicio && g.Fecha <= fechaFin);
            var gastosOperativos = gastosLista.Sum(g => g.Monto);

            // ── UTILIDAD OPERACIONAL ──────────────────────────────────────────────
            var utilidadOperacional    = utilidadBruta - gastosOperativos;
            var margenOperacional      = ventasNetas > 0 ? Math.Round((utilidadOperacional / ventasNetas) * 100, 2) : 0;

            // ── UTILIDAD NETA (separada para futuras líneas: impuestos, otros) ────
            var utilidadNeta = utilidadOperacional;
            var margenNeto   = ventasNetas > 0 ? Math.Round((utilidadNeta / ventasNetas) * 100, 2) : 0;

            // ── DATOS COMPLEMENTARIOS ─────────────────────────────────────────────
            var comprasDelPeriodo = (await _unitOfWork.Compras.FindAsync(
                c => c.FechaCompra >= fechaInicio && c.FechaCompra <= fechaFin && !c.Anulada))
                .Sum(c => c.Total);

            var productos = await _unitOfWork.Productos.GetAllAsync();
            var valorInventarioActual = productos.Sum(p => p.StockActual * p.PrecioCompra);

            return new EstadoResultadosDto
            {
                FechaInicio              = filtro.FechaInicio,
                FechaFin                 = filtro.FechaFin,
                VentasBrutas             = ventasBrutas,
                Devoluciones             = devoluciones,
                Descuentos               = descuentos,
                VentasNetas              = ventasNetas,
                CostoProductosVendidos   = costoProductosVendidos,
                MermasDelPeriodo         = mermasDelPeriodo,
                TotalCostoVentas         = totalCostoVentas,
                UtilidadBruta            = utilidadBruta,
                MargenBrutoPorcentaje    = margenBruto,
                GastosOperativos         = gastosOperativos,
                UtilidadOperacional      = utilidadOperacional,
                MargenOperacionalPorcentaje = margenOperacional,
                UtilidadNeta             = utilidadNeta,
                MargenNetoPorcentaje     = margenNeto,
                ComprasDelPeriodo        = comprasDelPeriodo,
                ValorInventarioActual    = valorInventarioActual,
            };
        }

        #endregion

        #region INVENTARIO

        public async Task<ResumenInventarioDto> ObtenerReporteInventarioAsync()
        {
            // ✅ Solo productos activos
            var todosProductos = await _unitOfWork.Productos.GetAllAsync();
            var productosActivos = todosProductos
                .Where(p => p.Estado == EstadoRegistro.Activo)
                .ToList();

            var inventarios = productosActivos.Select(p => new InventarioReporteDto
            {
                ProductoId = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Categoria = p.Categoria?.Nombre ?? "Sin categoría",
                StockActual = (int)p.StockActual,
                StockMinimo = (int)p.StockMinimo,
                PrecioCompra = p.PrecioCompra,
                PrecioVenta = p.PrecioVenta,
                ValorInventario = p.StockActual * p.PrecioCompra,
                EstadoStock = p.StockActual <= 0 ? "Sin Stock" 
                    : p.StockActual <= p.StockMinimo ? "Stock Bajo" 
                    : "Stock Normal",
                RotacionInventario = 0
            }).ToList();

            return new ResumenInventarioDto
            {
                TotalProductos = productosActivos.Count,
                ProductosConStock = inventarios.Count(i => i.StockActual > 0),
                ProductosSinStock = inventarios.Count(i => i.StockActual <= 0),
                ProductosStockBajo = inventarios.Count(i => i.EstadoStock == "Stock Bajo"),
                ValorTotalInventario = inventarios.Sum(i => i.ValorInventario),
                RotacionPromedio = inventarios.Any() 
                    ? Math.Round(inventarios.Average(i => i.RotacionInventario), 1) 
                    : 0,
                Productos = inventarios.OrderBy(i => i.Nombre).ToList()
            };
        }

        #endregion

        #region ANÁLISIS ABC

        public async Task<ResumenABCDto> ObtenerAnalisisABCAsync(FiltroReporteDto filtro)
        {
            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(
                filtro.FechaInicio.Date,
                filtro.FechaFin.Date
            );

            var ventasNoAnuladas = ventas.Where(v => !v.Anulada).ToList();

            // Agrupar ventas por producto
            var ventasPorProducto = ventasNoAnuladas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    Producto = g.First().Producto,
                    CantidadVendida = (int)g.Sum(d => d.Cantidad),
                    TotalVentas = g.Sum(d => d.Total),
                    TotalCosto = g.Sum(d => d.CostoUnitario * d.Cantidad),
                    UtilidadTotal = g.Sum(d => d.Total - (d.CostoUnitario * d.Cantidad))
                })
                .OrderByDescending(p => p.TotalVentas)
                .ToList();

            var ventasTotales = ventasPorProducto.Sum(p => p.TotalVentas);
            decimal acumulado = 0;

            var productos = new List<ProductoABCDto>();

            foreach (var item in ventasPorProducto)
            {
                var porcentaje = ventasTotales > 0 ? (item.TotalVentas / ventasTotales) * 100 : 0;
                acumulado += porcentaje;

                var clasificacion = acumulado <= 80 ? "A" 
                    : acumulado <= 95 ? "B" 
                    : "C";

                var margenUnitario = item.CantidadVendida > 0 
                    ? item.UtilidadTotal / item.CantidadVendida 
                    : 0;

                var rotacion = item.Producto?.StockActual > 0 
                    ? Math.Round((decimal)item.CantidadVendida / item.Producto.StockActual, 2)
                    : 0;

                productos.Add(new ProductoABCDto
                {
                    ProductoId = item.ProductoId,
                    Codigo = item.Producto?.Codigo ?? "",
                    Nombre = item.Producto?.Nombre ?? "Producto eliminado",
                    Categoria = item.Producto?.Categoria?.Nombre ?? "",
                    CantidadVendida = item.CantidadVendida,
                    TotalVentas = item.TotalVentas,
                    PorcentajeVentas = porcentaje,
                    PorcentajeAcumulado = acumulado,
                    ClasificacionABC = clasificacion,
                    MargenUnitario = margenUnitario,
                    UtilidadTotal = item.UtilidadTotal,
                    StockActual = (int)(item.Producto?.StockActual ?? 0),
                    RotacionInventario = rotacion
                });
            }

            var productosA = productos.Count(p => p.ClasificacionABC == "A");
            var productosB = productos.Count(p => p.ClasificacionABC == "B");
            var productosC = productos.Count(p => p.ClasificacionABC == "C");

            var ventasA = productos.Where(p => p.ClasificacionABC == "A").Sum(p => p.TotalVentas);
            var ventasB = productos.Where(p => p.ClasificacionABC == "B").Sum(p => p.TotalVentas);
            var ventasC = productos.Where(p => p.ClasificacionABC == "C").Sum(p => p.TotalVentas);

            return new ResumenABCDto
            {
                TotalProductos = productos.Count,
                ProductosA = productosA,
                ProductosB = productosB,
                ProductosC = productosC,
                VentasTotales = ventasTotales,
                VentasA = ventasA,
                VentasB = ventasB,
                VentasC = ventasC,
                PorcentajeA = ventasTotales > 0 ? (ventasA / ventasTotales) * 100 : 0,
                PorcentajeB = ventasTotales > 0 ? (ventasB / ventasTotales) * 100 : 0,
                PorcentajeC = ventasTotales > 0 ? (ventasC / ventasTotales) * 100 : 0,
                Productos = productos
            };
        }

        public async Task<byte[]> ExportarAnalisisABCAsync(FiltroReporteDto filtro)
        {
            var datos = await ObtenerAnalisisABCAsync(filtro);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Análisis ABC");

            // Título
            worksheet.Cell(1, 1).Value = "ANÁLISIS ABC DE PRODUCTOS";
            worksheet.Range(1, 1, 1, 11).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#9C27B0");
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;

            // Período
            worksheet.Cell(2, 1).Value = $"Período: {filtro.FechaInicio:dd/MM/yyyy} - {filtro.FechaFin:dd/MM/yyyy}";
            worksheet.Range(2, 1, 2, 11).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Resumen por clasificación
            worksheet.Cell(4, 1).Value = "RESUMEN POR CLASIFICACIÓN";
            worksheet.Cell(4, 1).Style.Font.Bold = true;
            worksheet.Cell(4, 1).Style.Font.FontSize = 14;

            // Headers resumen
            worksheet.Cell(5, 1).Value = "Clasificación";
            worksheet.Cell(5, 2).Value = "Productos";
            worksheet.Cell(5, 3).Value = "% Productos";
            worksheet.Cell(5, 4).Value = "Ventas";
            worksheet.Cell(5, 5).Value = "% Ventas";

            for (int i = 1; i <= 5; i++)
            {
                worksheet.Cell(5, i).Style.Font.Bold = true;
                worksheet.Cell(5, i).Style.Fill.BackgroundColor = XLColor.FromHtml("#E1BEE7");
            }

            // Datos resumen
            worksheet.Cell(6, 1).Value = "A";
            worksheet.Cell(6, 2).Value = datos.ProductosA;
            worksheet.Cell(6, 3).Value = datos.TotalProductos > 0 ? (decimal)datos.ProductosA / datos.TotalProductos : 0;
            worksheet.Cell(6, 4).Value = datos.VentasA;
            worksheet.Cell(6, 5).Value = datos.PorcentajeA / 100;
            worksheet.Cell(6, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4CAF50");

            worksheet.Cell(7, 1).Value = "B";
            worksheet.Cell(7, 2).Value = datos.ProductosB;
            worksheet.Cell(7, 3).Value = datos.TotalProductos > 0 ? (decimal)datos.ProductosB / datos.TotalProductos : 0;
            worksheet.Cell(7, 4).Value = datos.VentasB;
            worksheet.Cell(7, 5).Value = datos.PorcentajeB / 100;
            worksheet.Cell(7, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF9800");

            worksheet.Cell(8, 1).Value = "C";
            worksheet.Cell(8, 2).Value = datos.ProductosC;
            worksheet.Cell(8, 3).Value = datos.TotalProductos > 0 ? (decimal)datos.ProductosC / datos.TotalProductos : 0;
            worksheet.Cell(8, 4).Value = datos.VentasC;
            worksheet.Cell(8, 5).Value = datos.PorcentajeC / 100;
            worksheet.Cell(8, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F44336");

            for (int rowResumen = 6; rowResumen <= 8; rowResumen++)
            {
                worksheet.Cell(rowResumen, 3).Style.NumberFormat.Format = "0.00%";
                worksheet.Cell(rowResumen, 4).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(rowResumen, 5).Style.NumberFormat.Format = "0.00%";
            }

            worksheet.Cell(10, 1).Value = "DETALLE POR PRODUCTO";
            worksheet.Cell(10, 1).Style.Font.Bold = true;
            worksheet.Cell(10, 1).Style.Font.FontSize = 14;

            var headers = new[] { "Código", "Producto", "Categoría", "Clasificación", "Cant. Vendida", "Total Ventas", "% Ventas", "% Acumulado", "Utilidad", "Stock", "Rotación" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(11, i + 1).Value = headers[i];
                worksheet.Cell(11, i + 1).Style.Font.Bold = true;
                worksheet.Cell(11, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E1BEE7");
                worksheet.Cell(11, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            int row = 12;
            foreach (var producto in datos.Productos)
            {
                worksheet.Cell(row, 1).Value = producto.Codigo;
                worksheet.Cell(row, 2).Value = producto.Nombre;
                worksheet.Cell(row, 3).Value = producto.Categoria;
                worksheet.Cell(row, 4).Value = producto.ClasificacionABC;
                worksheet.Cell(row, 5).Value = producto.CantidadVendida;
                worksheet.Cell(row, 6).Value = producto.TotalVentas;
                worksheet.Cell(row, 7).Value = producto.PorcentajeVentas / 100;
                worksheet.Cell(row, 8).Value = producto.PorcentajeAcumulado / 100;
                worksheet.Cell(row, 9).Value = producto.UtilidadTotal;
                worksheet.Cell(row, 10).Value = producto.StockActual;
                worksheet.Cell(row, 11).Value = producto.RotacionInventario;

                // Formato
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "0.00%";
                worksheet.Cell(row, 8).Style.NumberFormat.Format = "0.00%";
                worksheet.Cell(row, 9).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 11).Style.NumberFormat.Format = "0.00";

                // Color por clasificación
                var color = producto.ClasificacionABC switch
                {
                    "A" => XLColor.FromHtml("#C8E6C9"),
                    "B" => XLColor.FromHtml("#FFE0B2"),
                    "C" => XLColor.FromHtml("#FFCDD2"),
                    _ => XLColor.White
                };
                worksheet.Cell(row, 4).Style.Fill.BackgroundColor = color;
                worksheet.Cell(row, 4).Style.Font.Bold = true;
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
            }

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }



        #endregion

        #region CAJA

        public async Task<ResumenCajaDto> ObtenerReporteCajaAsync(FiltroReporteDto filtro)
        {
            var cajas = await _unitOfWork.Cajas.ObtenerCajasPorRangoFechaAsync(
                filtro.FechaInicio, 
                filtro.FechaFin);

            var detalles = cajas.Select(c => new CajaReporteDto
            {
                CajaId = c.Id,
                FechaApertura = c.FechaApertura,
                FechaCierre = c.FechaCierre,
                Usuario = c.Usuario?.NombreCompleto ?? "",
                MontoApertura = c.MontoInicial,
                TotalVentas = c.Ventas?.Sum(v => v.Total) ?? 0,
                TotalRetiros = c.Retiros.Sum(r => r.Monto),
                EfectivoEsperado = c.EfectivoEsperado,
                EfectivoContado = c.EfectivoContado,
                Diferencia = c.Diferencia,
                Estado = c.FechaCierre.HasValue ? "Cerrada" : "Abierta"
            }).ToList();

            return new ResumenCajaDto
            {
                FechaInicio = filtro.FechaInicio,
                FechaFin = filtro.FechaFin,
                TotalArqueos = detalles.Count(),
                TotalAperturas = detalles.Sum(d => d.MontoApertura),
                TotalVentas = detalles.Sum(d => d.TotalVentas),
                TotalRetiros = detalles.Sum(d => d.TotalRetiros),
                TotalDiferencias = detalles.Sum(d => d.Diferencia),
                PromedioVentasPorCaja = detalles.Count() > 0 ? detalles.Average(d => d.TotalVentas) : 0,
                Detalles = detalles
            };
        }

        public async Task<ResumenArqueosDto> ObtenerReporteArqueosAsync(FiltroReporteDto filtro)
        {
            // ✅ Usar el método específico que carga las ventas
            var cajasFiltradas = await _unitOfWork.Cajas.GetCajasConVentasAsync(
                filtro.FechaInicio.Date,
                filtro.FechaFin.Date
            );

            var cajasList = cajasFiltradas.ToList();

            var arqueos = cajasList.Select(c =>
            {
                // ✅ Total de ventas = suma de todos los métodos de pago
                var totalVentas = c.TotalEfectivo + c.TotalTarjeta + c.TotalNequi + 
                                c.TotalDaviplata + c.TotalTransferencia + c.TotalQR;
                
                // ✅ Número de ventas (excluyendo anuladas) - ahora debería funcionar
                var numeroVentas = c.Ventas?.Count(v => !v.Anulada) ?? 0;
                
                // ✅ Monto esperado = Monto inicial + Total ventas - Retiros
                var montoEsperado = c.MontoInicial + totalVentas - c.TotalRetiros;
                
                // ✅ Diferencia = Contado - Esperado
                // Positivo = Excedente, Negativo = Faltante
                var diferencia = c.MontoFinal - montoEsperado;

                return new ArqueoDetalleDto
                {
                    ArqueoId = c.Id,
                    FechaApertura = c.FechaApertura,
                    FechaCierre = c.FechaCierre,
                    NombreUsuario = c.Usuario?.NombreCompleto ?? "Usuario",
                    MontoInicial = c.MontoInicial,
                    TotalVentas = totalVentas,
                    TotalEfectivo = c.TotalEfectivo,
                    TotalTarjeta = c.TotalTarjeta,
                    TotalTransferencia = c.TotalTransferencia + c.TotalNequi + c.TotalDaviplata + c.TotalQR,
                    MontoEsperado = montoEsperado,
                    MontoContado = c.MontoFinal,
                    Diferencia = diferencia,
                    EstadoCaja = c.Abierta ? "Abierta" : "Cerrada",
                    NumeroVentas = numeroVentas,  // ✅ Ahora debería mostrar el número correcto
                    VentaPromedio = numeroVentas > 0 && c.Ventas != null
                        ? c.Ventas.Where(v => !v.Anulada).Average(v => v.Total)
                        : 0
                };
            }).ToList();

            var totalIngresos = arqueos.Sum(a => a.TotalVentas);
            var totalRetiros = cajasList.Sum(c => c.TotalRetiros);
            
            return new ResumenArqueosDto
            {
                TotalArqueos = arqueos.Count,
                TotalIngresos = totalIngresos,
                TotalEgresos = totalRetiros,
                SaldoNeto = totalIngresos - totalRetiros,
                DiferenciaTotal = arqueos.Sum(a => a.Diferencia),
                Arqueos = arqueos
            };
        }

        #endregion

        #region DASHBOARD
        public async Task<DashboardPrincipalDto> ObtenerDashboardPrincipalAsync()
        {
            var hoy = DateTime.Now.Date;
            var ayer = hoy.AddDays(-1);
            var hace7Dias = hoy.AddDays(-7);
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            // Ventas de hoy
            var ventasHoy = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(hoy, hoy);
            var ventasHoyActivas = ventasHoy.Where(v => !v.Anulada).ToList();
            var totalVentasHoy = ventasHoyActivas.Sum(v => v.Total);
            var transaccionesHoy = ventasHoyActivas.Count;

            // Ventas de ayer para comparación
            var ventasAyer = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(ayer, ayer);
            var totalVentasAyer = ventasAyer.Where(v => !v.Anulada).Sum(v => v.Total);
            var crecimientoVsAyer = totalVentasAyer > 0 
                ? ((totalVentasHoy - totalVentasAyer) / totalVentasAyer) * 100 
                : 0;

            // Estado de caja actual
            var cajas = await _unitOfWork.Cajas.GetAllAsync();
            var cajaAbierta = cajas.FirstOrDefault(c => c.Abierta);

            // Ventas últimos 7 días
            var ventasSemanales = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(hace7Dias, hoy);
            var ventasPorDia = ventasSemanales
                .Where(v => !v.Anulada)
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new VentaDiariaDto
                {
                    Fecha = g.Key,
                    Total = g.Sum(v => v.Total),
                    Transacciones = g.Count()
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            // Top 5 productos del día
            var topProductos = ventasHoyActivas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.ProductoId)
                .Select(g => new TopProductoDto
                {
                    Nombre = g.First().Producto?.Nombre ?? "Producto",
                    CantidadVendida = (int)g.Sum(d => d.Cantidad),
                    Total = g.Sum(d => d.Total)
                })
                .OrderByDescending(p => p.Total)
                .Take(5)
                .ToList();

            // Productos con stock bajo
            var productos = await _unitOfWork.Productos.GetAllAsync();
            var productosActivos = productos.Where(p => p.Estado == EstadoRegistro.Activo).ToList();
            var productosStockBajo = productosActivos.Count(p => p.StockActual <= p.StockMinimo && p.StockActual > 0);

            // Ventas anuladas hoy
            var ventasAnuladasHoy = ventasHoy.Count(v => v.Anulada);

            // Ventas de la semana
            var ventasSemana = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(inicioSemana, hoy);
            var totalVentasSemana = ventasSemana.Where(v => !v.Anulada).Sum(v => v.Total);

            // Ventas del mes
            var ventasMes = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(inicioMes, hoy);
            var totalVentasMes = ventasMes.Where(v => !v.Anulada).Sum(v => v.Total);

            // Semana anterior para comparación
            var inicioSemanaAnterior = inicioSemana.AddDays(-7);
            var finSemanaAnterior = inicioSemana.AddDays(-1);
            var ventasSemanaAnterior = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(inicioSemanaAnterior, finSemanaAnterior);
            var totalSemanaAnterior = ventasSemanaAnterior.Where(v => !v.Anulada).Sum(v => v.Total);
            var crecimientoSemanal = totalSemanaAnterior > 0 
                ? ((totalVentasSemana - totalSemanaAnterior) / totalSemanaAnterior) * 100 
                : 0;

            // Mes anterior para comparación
            var inicioMesAnterior = inicioMes.AddMonths(-1);
            var finMesAnterior = inicioMes.AddDays(-1);
            var ventasMesAnterior = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(inicioMesAnterior, finMesAnterior);
            var totalMesAnterior = ventasMesAnterior.Where(v => !v.Anulada).Sum(v => v.Total);
            var crecimientoMensual = totalMesAnterior > 0 
                ? ((totalVentasMes - totalMesAnterior) / totalMesAnterior) * 100 
                : 0;

            return new DashboardPrincipalDto
            {
                VentasHoy = totalVentasHoy,
                TransaccionesHoy = transaccionesHoy,
                TicketPromedio = transaccionesHoy > 0 ? totalVentasHoy / transaccionesHoy : 0,
                CrecimientoVsAyer = crecimientoVsAyer,
                
                CajaAbierta = cajaAbierta != null,
                UsuarioCaja = cajaAbierta?.Usuario?.NombreCompleto,
                MontoCajaActual = cajaAbierta?.MontoInicial ?? 0,
                FechaAperturaCaja = cajaAbierta?.FechaApertura,
                
                ProductosStockBajo = productosStockBajo,
                VentasAnuladasHoy = ventasAnuladasHoy,
                
                TopProductos = topProductos,
                VentasSemanales = ventasPorDia,
                
                VentasSemana = totalVentasSemana,
                VentasMes = totalVentasMes,
                CrecimientoSemanal = crecimientoSemanal,
                CrecimientoMensual = crecimientoMensual
            };
        }

        public async Task<DashboardVentasDto> ObtenerDashboardVentasAsync(string periodo)
        {
            var hoy = DateTime.Now.Date;
            DateTime fechaInicio, fechaFin, fechaInicioAnterior, fechaFinAnterior;

            switch (periodo)
            {
                case "semana":
                    fechaInicio = hoy.AddDays(-6);
                    fechaFin = hoy;
                    fechaInicioAnterior = hoy.AddDays(-13);
                    fechaFinAnterior = hoy.AddDays(-7);
                    break;
                case "mes":
                    fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    fechaFin = hoy;
                    var mesAnteriorInicio = fechaInicio.AddMonths(-1);
                    fechaInicioAnterior = new DateTime(mesAnteriorInicio.Year, mesAnteriorInicio.Month, 1);
                    fechaFinAnterior = fechaInicio.AddDays(-1);
                    break;
                case "año":
                    fechaInicio = new DateTime(hoy.Year, 1, 1);
                    fechaFin = hoy;
                    fechaInicioAnterior = new DateTime(hoy.Year - 1, 1, 1);
                    fechaFinAnterior = new DateTime(hoy.Year - 1, 12, 31);
                    break;
                default: // "dia"
                    fechaInicio = hoy;
                    fechaFin = hoy;
                    fechaInicioAnterior = hoy.AddDays(-1);
                    fechaFinAnterior = hoy.AddDays(-1);
                    break;
            }

            var ventas = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(fechaInicio, fechaFin);
            var ventasActivas = ventas.Where(v => !v.Anulada).ToList();
            var totalPeriodo = ventasActivas.Sum(v => v.Total);
            var transacciones = ventasActivas.Count;

            var ventasAnterior = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(fechaInicioAnterior, fechaFinAnterior);
            var totalAnterior = ventasAnterior.Where(v => !v.Anulada).Sum(v => v.Total);
            var crecimiento = totalAnterior > 0 ? Math.Round(((totalPeriodo - totalAnterior) / totalAnterior) * 100, 1) : 0;

            var tendencia = ventasActivas
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new VentaDiariaDto
                {
                    Fecha = g.Key,
                    Total = g.Sum(v => v.Total),
                    Transacciones = g.Count()
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            var ventasPorCategoria = ventasActivas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.Producto?.Categoria?.Nombre ?? "Sin categoría")
                .Select(g => new VentasCategoriaDto
                {
                    NombreCategoria = g.Key,
                    Total = g.Sum(d => d.Total),
                    Transacciones = g.Select(d => d.VentaId).Distinct().Count()
                })
                .OrderByDescending(c => c.Total)
                .Take(8)
                .ToList();

            var totalCat = ventasPorCategoria.Sum(c => c.Total);
            foreach (var cat in ventasPorCategoria)
                cat.Porcentaje = totalCat > 0 ? Math.Round((cat.Total / totalCat) * 100, 1) : 0;

            var metodosPago = ventasActivas
                .SelectMany(v => v.Pagos)
                .GroupBy(p => p.Metodo.ToString())
                .Select(g => new MetodoPagoResumenDto
                {
                    MetodoPago = g.Key,
                    Total = g.Sum(p => p.Monto),
                    Cantidad = g.Count()
                })
                .OrderByDescending(m => m.Total)
                .ToList();

            var totalPagos = metodosPago.Sum(m => m.Total);
            foreach (var mp in metodosPago)
                mp.Porcentaje = totalPagos > 0 ? Math.Round((mp.Total / totalPagos) * 100, 1) : 0;

            return new DashboardVentasDto
            {
                VentasPeriodo = totalPeriodo,
                VentasPeriodoAnterior = totalAnterior,
                CrecimientoPeriodo = crecimiento,
                TransaccionesPeriodo = transacciones,
                TicketPromedioPeriodo = transacciones > 0 ? Math.Round(totalPeriodo / transacciones, 0) : 0,
                VentasTendencia = tendencia,
                VentasPorCategoria = ventasPorCategoria,
                MetodosPago = metodosPago
            };
        }

        public async Task<DashboardInventarioDto> ObtenerDashboardInventarioAsync()
        {
            var activos = (await _unitOfWork.Productos.GetProductosActivosAsync()).ToList();

            var totalProductos = activos.Count;
            var conStock = activos.Count(p => p.StockActual > 0);
            var sinStock = activos.Count(p => p.StockActual <= 0);
            var stockBajo = activos.Count(p => p.StockActual > 0 && p.StockActual <= p.StockMinimo);
            var stockCritico = activos.Count(p => p.StockActual > 0 && p.StockMinimo > 0 && p.StockActual <= p.StockMinimo * 0.5m);
            var valorTotal = activos.Sum(p => p.StockActual * p.PrecioCompra);

            var stockPorCategoria = activos
                .GroupBy(p => p.Categoria?.Nombre ?? "Sin categoría")
                .Select(g => new StockCategoriaDto
                {
                    NombreCategoria = g.Key,
                    TotalProductos = g.Count(),
                    UnidadesStock = (int)g.Sum(p => p.StockActual),
                    ValorTotal = g.Sum(p => p.StockActual * p.PrecioCompra)
                })
                .OrderByDescending(c => c.ValorTotal)
                .ToList();

            var productosCriticos = activos
                .Where(p => p.StockActual <= p.StockMinimo)
                .Select(p => new ProductoCriticoDto
                {
                    Nombre = p.Nombre,
                    Categoria = p.Categoria?.Nombre ?? "Sin categoría",
                    StockActual = (int)p.StockActual,
                    StockMinimo = (int)p.StockMinimo,
                    EstadoStock = p.StockActual <= 0 ? "Sin stock"
                                 : p.StockMinimo > 0 && p.StockActual <= p.StockMinimo * 0.5m ? "Crítico"
                                 : "Bajo"
                })
                .OrderBy(p => p.StockActual)
                .Take(20)
                .ToList();

            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var ventasMes = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(inicioMes, DateTime.Now.Date);
            var masRotados = ventasMes
                .Where(v => !v.Anulada)
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.ProductoId)
                .Select(g => new TopProductoDto
                {
                    Nombre = g.First().Producto?.Nombre ?? "Producto",
                    CantidadVendida = (int)g.Sum(d => d.Cantidad),
                    Total = g.Sum(d => d.Total)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(10)
                .ToList();

            return new DashboardInventarioDto
            {
                TotalProductos = totalProductos,
                ProductosConStock = conStock,
                ProductosSinStock = sinStock,
                ProductosStockBajo = stockBajo,
                ProductosStockCritico = stockCritico,
                ValorTotalInventario = valorTotal,
                StockPorCategoria = stockPorCategoria,
                ProductosCriticos = productosCriticos,
                ProductosMasRotados = masRotados
            };
        }

        public async Task<DashboardFinancieroDto> ObtenerDashboardFinancieroAsync()
        {
            var hoy = DateTime.Now.Date;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var diasTranscurridos = Math.Max(1, (hoy - inicioMes).Days + 1);
            var diasDelMes = DateTime.DaysInMonth(hoy.Year, hoy.Month);

            var ventasMes = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(inicioMes, hoy);
            var ventasActivas = ventasMes.Where(v => !v.Anulada).ToList();
            var ingresosDelMes = ventasActivas.Sum(v => v.Total);
            var costoProductosVendidos = ventasActivas.Sum(v => v.Detalles.Sum(d => d.CostoUnitario * d.Cantidad));

            var ventasDiarias = ventasActivas
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new VentaDiariaDto
                {
                    Fecha = g.Key,
                    Total = g.Sum(v => v.Total),
                    Transacciones = g.Count()
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            var comprasMes = await _unitOfWork.Compras.FindAsync(
                c => c.FechaCompra >= inicioMes && c.FechaCompra <= hoy && !c.Anulada);
            var comprasDelMes = comprasMes.Sum(c => c.Total);

            var mermasMes = await _unitOfWork.Mermas.FindAsync(
                m => m.Fecha >= inicioMes && m.Fecha <= hoy);
            var mermasDelMes = mermasMes.Sum(m => m.CostoTotal);

            var gastosMes = await _unitOfWork.Gastos.FindAsync(
                g => g.Fecha >= inicioMes && g.Fecha <= hoy);
            var gastosDelMes = gastosMes.Sum(g => g.Monto);

            var totalCostos = costoProductosVendidos + mermasDelMes;
            var utilidadBruta = ingresosDelMes - totalCostos;
            var margenBruto = ingresosDelMes > 0 ? Math.Round((utilidadBruta / ingresosDelMes) * 100, 1) : 0;
            var utilidadNeta = utilidadBruta - gastosDelMes;

            var inicioMesAnterior = inicioMes.AddMonths(-1);
            var finMesAnterior = inicioMes.AddDays(-1);
            var ventasMesAnterior = await _unitOfWork.Ventas.GetVentasPorFechaRangoAsync(inicioMesAnterior, finMesAnterior);
            var totalMesAnterior = ventasMesAnterior.Where(v => !v.Anulada).Sum(v => v.Total);
            var crecimiento = totalMesAnterior > 0
                ? Math.Round(((ingresosDelMes - totalMesAnterior) / totalMesAnterior) * 100, 1)
                : 0;

            var promedioDiario = ingresosDelMes / diasTranscurridos;
            var proyeccionMes = Math.Round(promedioDiario * diasDelMes, 0);

            var flujoCaja = ventasActivas
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new FlujoCajaDto
                {
                    Fecha = g.Key,
                    Ingresos = g.Sum(v => v.Total),
                    Egresos = 0m
                })
                .OrderBy(f => f.Fecha)
                .ToList();

            return new DashboardFinancieroDto
            {
                IngresosDelMes = ingresosDelMes,
                CostoProductosVendidos = costoProductosVendidos,
                ComprasDelMes = comprasDelMes,
                MermasDelMes = mermasDelMes,
                GastosOperativosDelMes = gastosDelMes,
                UtilidadBruta = utilidadBruta,
                MargenBruto = margenBruto,
                UtilidadNeta = utilidadNeta,
                VentasMesAnterior = totalMesAnterior,
                CrecimientoVsMesAnterior = crecimiento,
                ProyeccionMes = proyeccionMes,
                VentasMensuales = ventasDiarias,
                FlujoCaja = flujoCaja
            };
        }

        #endregion

        #region EXPORTACIÓN A EXCEL

        public async Task<byte[]> ExportarVentasPorPeriodoAsync(FiltroReporteDto filtro)
        {
            var reporte = await ObtenerVentasPorPeriodoAsync(filtro);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ventas por Período");

            // Título
            worksheet.Cell(1, 1).Value = "REPORTE DE VENTAS POR PERÍODO";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 7).Merge();

            // Información del reporte
            worksheet.Cell(2, 1).Value = $"Período: {reporte.FechaInicio:dd/MM/yyyy} - {reporte.FechaFin:dd/MM/yyyy}";
            worksheet.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            // Resumen
            worksheet.Cell(5, 1).Value = "RESUMEN EJECUTIVO";
            worksheet.Cell(5, 1).Style.Font.Bold = true;
            worksheet.Cell(5, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            worksheet.Cell(6, 1).Value = "Total Ventas:";
            worksheet.Cell(6, 2).Value = reporte.TotalVentas;
            
            worksheet.Cell(7, 1).Value = "Total Ingresos:";
            worksheet.Cell(7, 2).Value = reporte.TotalIngresos;
            worksheet.Cell(7, 2).Style.NumberFormat.Format = "$#,##0.00";
            
            worksheet.Cell(8, 1).Value = "Ticket Promedio:";
            worksheet.Cell(8, 2).Value = reporte.TicketPromedio;
            worksheet.Cell(8, 2).Style.NumberFormat.Format = "$#,##0.00";
            
            worksheet.Cell(9, 1).Value = "Utilidad Bruta:";
            worksheet.Cell(9, 2).Value = reporte.UtilidadBruta;
            worksheet.Cell(9, 2).Style.NumberFormat.Format = "$#,##0.00";
            
            worksheet.Cell(10, 1).Value = "Margen %:";
            worksheet.Cell(10, 2).Value = reporte.MargenPorcentaje / 100;
            worksheet.Cell(10, 2).Style.NumberFormat.Format = "0.00%";
            
            worksheet.Cell(11, 1).Value = "Crecimiento:";
            worksheet.Cell(11, 2).Value = reporte.CrecimientoVsPeriodoAnterior / 100;
            worksheet.Cell(11, 2).Style.NumberFormat.Format = "0.00%";

            // Encabezados detalle
            var headerRow = 13;
            worksheet.Cell(headerRow, 1).Value = "Fecha";
            worksheet.Cell(headerRow, 2).Value = "Cant. Ventas";
            worksheet.Cell(headerRow, 3).Value = "Total Ventas";
            worksheet.Cell(headerRow, 4).Value = "Ticket Promedio";
            worksheet.Cell(headerRow, 5).Value = "Costo";
            worksheet.Cell(headerRow, 6).Value = "Utilidad";
            worksheet.Cell(headerRow, 7).Value = "Margen %";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Datos
            var currentRow = headerRow + 1;
            foreach (var detalle in reporte.DetallesPorDia)
            {
                worksheet.Cell(currentRow, 1).Value = detalle.Fecha.ToString("dd/MM/yyyy");
                worksheet.Cell(currentRow, 2).Value = detalle.CantidadVentas;
                worksheet.Cell(currentRow, 3).Value = detalle.TotalVentas;
                worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(currentRow, 4).Value = detalle.TicketPromedio;
                worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(currentRow, 5).Value = detalle.TotalCosto;
                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(currentRow, 6).Value = detalle.UtilidadBruta;
                worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(currentRow, 7).Value = detalle.MargenPorcentaje / 100;
                worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "0.00%";
                currentRow++;
            }

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportarReporteArqueosAsync(FiltroReporteDto filtro)
        {
            var datos = await ObtenerReporteArqueosAsync(filtro);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte de Arqueos");

            // Título
            worksheet.Cell(1, 1).Value = "REPORTE DE ARQUEOS DE CAJA";
            worksheet.Range(1, 1, 1, 12).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC107");
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;

            // Período
            worksheet.Cell(2, 1).Value = $"Período: {filtro.FechaInicio:dd/MM/yyyy} - {filtro.FechaFin:dd/MM/yyyy}";
            worksheet.Range(2, 1, 2, 12).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Resumen
            worksheet.Cell(3, 1).Value = $"Total Arqueos: {datos.TotalArqueos} | Ingresos: {datos.TotalIngresos:C} | Diferencia Total: {datos.DiferenciaTotal:C}";
            worksheet.Range(3, 1, 3, 12).Merge();
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Headers
            var headers = new[] { "Fecha Apertura", "Fecha Cierre", "Usuario", "Monto Inicial", "Efectivo", "Tarjeta", "Digital", "Total Ventas", "Esperado", "Contado", "Diferencia", "Estado" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(5, i + 1).Value = headers[i];
                worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF3E0");
                worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int rowData = 6;
            foreach (var arqueo in datos.Arqueos)
            {
                worksheet.Cell(rowData, 1).Value = arqueo.FechaApertura;
                worksheet.Cell(rowData, 2).Value = arqueo.FechaCierre ?? (DateTime?)null;
                worksheet.Cell(rowData, 3).Value = arqueo.NombreUsuario;
                worksheet.Cell(rowData, 4).Value = arqueo.MontoInicial;
                worksheet.Cell(rowData, 5).Value = arqueo.TotalEfectivo;
                worksheet.Cell(rowData, 6).Value = arqueo.TotalTarjeta;
                worksheet.Cell(rowData, 7).Value = arqueo.TotalTransferencia;
                worksheet.Cell(rowData, 8).Value = arqueo.TotalVentas;
                worksheet.Cell(rowData, 9).Value = arqueo.MontoEsperado;
                worksheet.Cell(rowData, 10).Value = arqueo.MontoContado;
                worksheet.Cell(rowData, 11).Value = arqueo.Diferencia;
                worksheet.Cell(rowData, 12).Value = arqueo.EstadoCaja;

                // Formato
                worksheet.Cell(rowData, 1).Style.NumberFormat.Format = "dd/MM/yyyy HH:mm";
                if (arqueo.FechaCierre.HasValue)
                    worksheet.Cell(rowData, 2).Style.NumberFormat.Format = "dd/MM/yyyy HH:mm";
                
                for (int col = 4; col <= 11; col++)
                {
                    worksheet.Cell(rowData, col).Style.NumberFormat.Format = "$#,##0.00";
                }

                // Color diferencia
                if (arqueo.Diferencia != 0)
                {
                    worksheet.Cell(rowData, 11).Style.Font.Bold = true;
                    worksheet.Cell(rowData, 11).Style.Font.FontColor = arqueo.Diferencia > 0 
                        ? XLColor.Green 
                        : XLColor.Red;
                }

                rowData++;
            }

            // Totales
            worksheet.Cell(rowData, 1).Value = "TOTALES";
            worksheet.Cell(rowData, 1).Style.Font.Bold = true;
            worksheet.Range(rowData, 1, rowData, 3).Merge();
            worksheet.Cell(rowData, 4).Value = datos.Arqueos.Sum(a => a.MontoInicial);
            worksheet.Cell(rowData, 5).Value = datos.Arqueos.Sum(a => a.TotalEfectivo);
            worksheet.Cell(rowData, 6).Value = datos.Arqueos.Sum(a => a.TotalTarjeta);
            worksheet.Cell(rowData, 7).Value = datos.Arqueos.Sum(a => a.TotalTransferencia);
            worksheet.Cell(rowData, 8).Value = datos.TotalIngresos;
            worksheet.Cell(rowData, 9).Value = datos.Arqueos.Sum(a => a.MontoEsperado);
            worksheet.Cell(rowData, 10).Value = datos.Arqueos.Sum(a => a.MontoContado);
            worksheet.Cell(rowData, 11).Value = datos.DiferenciaTotal;

            worksheet.Range(rowData, 1, rowData, 12).Style.Font.Bold = true;
            worksheet.Range(rowData, 1, rowData, 12).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF8E1");
            
            for (int col = 4; col <= 11; col++)
            {
                worksheet.Cell(rowData, col).Style.NumberFormat.Format = "$#,##0.00";
            }

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        
        public async Task<byte[]> ExportarVentasAnuladasAsync(FiltroReporteDto filtro)
        {
            var datos = await ObtenerVentasAnuladasAsync(filtro);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ventas Anuladas");

            // Título
            worksheet.Cell(1, 1).Value = "REPORTE DE VENTAS ANULADAS";
            worksheet.Range(1, 1, 1, 10).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F44336");
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;

            // Período
            worksheet.Cell(2, 1).Value = $"Período: {filtro.FechaInicio:dd/MM/yyyy} - {filtro.FechaFin:dd/MM/yyyy}";
            worksheet.Range(2, 1, 2, 10).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Resumen
            worksheet.Cell(3, 1).Value = $"Total Anuladas: {datos.TotalVentasAnuladas} | % Anulación: {datos.PorcentajeAnulacion:N2}% | Monto Perdido: {datos.MontoTotalAnulado:C}";
            worksheet.Range(3, 1, 3, 10).Merge();
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Headers
            var headers = new[] { "# Venta", "Fecha Venta", "Fecha Anulación", "Cliente", "Usuario", "Monto", "Método Pago", "Tiempo (min)", "Productos", "Motivo" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(5, i + 1).Value = headers[i];
                worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFCDD2");
                worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int rowData = 6;
            foreach (var venta in datos.Ventas)
            {
                worksheet.Cell(rowData, 1).Value = venta.NumeroVenta;
                worksheet.Cell(rowData, 2).Value = venta.FechaVenta;
                worksheet.Cell(rowData, 3).Value = venta.FechaAnulacion;
                worksheet.Cell(rowData, 4).Value = venta.Cliente;
                worksheet.Cell(rowData, 5).Value = venta.Usuario;
                worksheet.Cell(rowData, 6).Value = venta.Total;
                worksheet.Cell(rowData, 7).Value = venta.MetodoPago;
                worksheet.Cell(rowData, 8).Value = venta.TiempoHastaAnulacion;
                worksheet.Cell(rowData, 9).Value = venta.CantidadProductos;
                worksheet.Cell(rowData, 10).Value = venta.MotivoAnulacion;

                // Formato
                worksheet.Cell(rowData, 2).Style.NumberFormat.Format = "dd/MM/yyyy HH:mm";
                worksheet.Cell(rowData, 3).Style.NumberFormat.Format = "dd/MM/yyyy HH:mm";
                worksheet.Cell(rowData, 6).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(rowData, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(rowData, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                rowData++;
            }

            // Totales
            worksheet.Cell(rowData, 1).Value = "TOTALES";
            worksheet.Cell(rowData, 1).Style.Font.Bold = true;
            worksheet.Range(rowData, 1, rowData, 5).Merge();
            worksheet.Cell(rowData, 6).Value = datos.MontoTotalAnulado;
            worksheet.Cell(rowData, 9).Value = datos.Ventas.Sum(v => v.CantidadProductos);

            worksheet.Range(rowData, 1, rowData, 10).Style.Font.Bold = true;
            worksheet.Range(rowData, 1, rowData, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFEBEE");
            worksheet.Cell(rowData, 6).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(rowData, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportarVentasPorProductoAsync(FiltroReporteDto filtro)
        {
            var reporte = await ObtenerVentasPorProductoAsync(filtro);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ventas por Producto");

            // Título y headers similares al anterior...
            worksheet.Cell(1, 1).Value = "REPORTE DE VENTAS POR PRODUCTO";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;

            var headerRow = 3;
            worksheet.Cell(headerRow, 1).Value = "Código";
            worksheet.Cell(headerRow, 2).Value = "Producto";
            worksheet.Cell(headerRow, 3).Value = "Categoría";
            worksheet.Cell(headerRow, 4).Value = "Cantidad";
            worksheet.Cell(headerRow, 5).Value = "Total Ventas";
            worksheet.Cell(headerRow, 6).Value = "Costo";
            worksheet.Cell(headerRow, 7).Value = "Utilidad";
            worksheet.Cell(headerRow, 8).Value = "Margen %";
            worksheet.Cell(headerRow, 9).Value = "% del Total";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            var currentRow = headerRow + 1;
            foreach (var item in reporte)
            {
                worksheet.Cell(currentRow, 1).Value = item.Codigo;
                worksheet.Cell(currentRow, 2).Value = item.Nombre;
                worksheet.Cell(currentRow, 3).Value = item.Categoria;
                worksheet.Cell(currentRow, 4).Value = item.CantidadVendida;
                worksheet.Cell(currentRow, 5).Value = item.TotalVentas;
                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(currentRow, 6).Value = item.CostoTotal;
                worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(currentRow, 7).Value = item.UtilidadBruta;
                worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(currentRow, 8).Value = item.MargenPorcentaje / 100;
                worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "0.00%";
                worksheet.Cell(currentRow, 9).Value = item.PorcentajeDelTotal / 100;
                worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "0.00%";
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportarEstadoResultadosAsync(FiltroReporteDto filtro)
        {
            var r = await ObtenerEstadoResultadosAsync(filtro);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Estado de Resultados");

            // ── Encabezado ────────────────────────────────────────────────────────
            ws.Range(1, 1, 1, 3).Merge();
            ws.Cell(1, 1).Value = "ESTADO DE RESULTADOS";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4CAF50");
            ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, 3).Merge();
            ws.Cell(2, 1).Value = $"Período: {r.FechaInicio:dd/MM/yyyy} al {r.FechaFin:dd/MM/yyyy}";
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            void AddSection(ref int row, string titulo, XLColor color)
            {
                row++;
                ws.Cell(row, 1).Value = titulo;
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Font.FontSize = 12;
                ws.Cell(row, 1).Style.Fill.BackgroundColor = color;
                ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                ws.Range(row, 1, row, 3).Merge();
                row++;
            }

            void AddRow(ref int row, string label, decimal value, bool isBold = false, bool isNegative = false, string? porcentaje = null)
            {
                ws.Cell(row, 1).Value = label;
                ws.Cell(row, 2).Value = isNegative ? -value : value;
                ws.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                if (porcentaje != null) ws.Cell(row, 3).Value = porcentaje;
                if (isBold)
                {
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 2).Style.Font.Bold = true;
                }
                row++;
            }

            void AddSeparator(ref int row)
            {
                ws.Range(row, 1, row, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                row++;
            }

            var row = 3;

            // INGRESOS
            AddSection(ref row, "INGRESOS", XLColor.FromHtml("#388E3C"));
            AddRow(ref row, "Ventas Brutas", r.VentasBrutas);
            AddRow(ref row, "(-) Devoluciones / Anulaciones", r.Devoluciones, isNegative: true);
            AddRow(ref row, "(-) Descuentos", r.Descuentos, isNegative: true);
            AddSeparator(ref row);
            AddRow(ref row, "VENTAS NETAS", r.VentasNetas, isBold: true);

            // COSTO DE VENTAS
            AddSection(ref row, "COSTO DE VENTAS  (método perpetuo)", XLColor.FromHtml("#E65100"));
            AddRow(ref row, "Costo de Productos Vendidos", r.CostoProductosVendidos, isNegative: true);
            AddRow(ref row, "(+) Mermas del Período", r.MermasDelPeriodo, isNegative: true);
            AddSeparator(ref row);
            AddRow(ref row, "TOTAL COSTO DE VENTAS", r.TotalCostoVentas, isBold: true, isNegative: true);

            // UTILIDAD BRUTA
            row++;
            ws.Cell(row, 1).Value = "UTILIDAD BRUTA";
            ws.Cell(row, 2).Value = r.UtilidadBruta;
            ws.Cell(row, 3).Value = $"{r.MargenBrutoPorcentaje:N2}%";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9");
            ws.Cell(row, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9");
            row++;

            // GASTOS OPERATIVOS
            AddSection(ref row, "GASTOS OPERATIVOS", XLColor.FromHtml("#C62828"));
            AddRow(ref row, "Total Gastos Operativos", r.GastosOperativos, isNegative: true);
            AddSeparator(ref row);
            AddRow(ref row, "UTILIDAD OPERACIONAL", r.UtilidadOperacional, isBold: true,
                porcentaje: $"{r.MargenOperacionalPorcentaje:N2}%");

            // UTILIDAD NETA
            row++;
            ws.Cell(row, 1).Value = "UTILIDAD NETA";
            ws.Cell(row, 2).Value = r.UtilidadNeta;
            ws.Cell(row, 3).Value = $"Margen neto: {r.MargenNetoPorcentaje:N2}%";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 13;
            ws.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");
            ws.Cell(row, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");
            row += 2;

            // DATOS COMPLEMENTARIOS
            AddSection(ref row, "DATOS COMPLEMENTARIOS", XLColor.FromHtml("#546E7A"));
            AddRow(ref row, "Compras a Proveedores (período)", r.ComprasDelPeriodo);
            AddRow(ref row, "Valor Inventario Actual", r.ValorInventarioActual);

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportarVentasPorClienteAsync(FiltroReporteDto filtro)
        {
            var datos = await ObtenerVentasPorClienteAsync(filtro);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ventas por Cliente");

            // Título
            worksheet.Cell(1, 1).Value = "REPORTE DE VENTAS POR CLIENTE";
            worksheet.Range(1, 1, 1, 7).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#9C27B0");
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;

            // Período
            worksheet.Cell(2, 1).Value = $"Período: {filtro.FechaInicio:dd/MM/yyyy} - {filtro.FechaFin:dd/MM/yyyy}";
            worksheet.Range(2, 1, 2, 7).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Headers
            var headers = new[] { "Cliente", "Documento", "# Compras", "Total Compras", "Ticket Promedio", "Última Compra", "Días" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(4, i + 1).Value = headers[i];
                worksheet.Cell(4, i + 1).Style.Font.Bold = true;
                worksheet.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E1BEE7");
                worksheet.Cell(4, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int row = 5;
            foreach (var cliente in datos)
            {
                worksheet.Cell(row, 1).Value = cliente.ClienteNombre;
                worksheet.Cell(row, 2).Value = cliente.NumeroDocumento;
                worksheet.Cell(row, 3).Value = cliente.CantidadCompras;
                worksheet.Cell(row, 4).Value = cliente.TotalCompras;
                worksheet.Cell(row, 5).Value = cliente.TicketPromedio;
                worksheet.Cell(row, 6).Value = cliente.UltimaCompra;
                worksheet.Cell(row, 7).Value = cliente.DiasDesdeUltimaCompra;

                // Formato
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "dd/MM/yyyy";
                worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
            }



            // Totales
            worksheet.Cell(row, 1).Value = "TOTALES";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 3).Value = datos.Sum(c => c.CantidadCompras);
            worksheet.Cell(row, 4).Value = datos.Sum(c => c.TotalCompras);
            worksheet.Cell(row, 5).Value = datos.Any() ? datos.Average(c => c.TicketPromedio) : 0;

            worksheet.Range(row, 1, row, 7).Style.Font.Bold = true;
            worksheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#F3E5F5");
            worksheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportarDesempenoUsuariosAsync(FiltroReporteDto filtro)
        {
            var datos = await ObtenerDesempenoUsuariosAsync(filtro);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Desempeño Usuarios");

            // Título
            worksheet.Cell(1, 1).Value = "REPORTE DE DESEMPEÑO POR USUARIO";
            worksheet.Range(1, 1, 1, 17).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2196F3");
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;

            // Período
            worksheet.Cell(2, 1).Value = $"Período: {filtro.FechaInicio:dd/MM/yyyy} - {filtro.FechaFin:dd/MM/yyyy}";
            worksheet.Range(2, 1, 2, 17).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Resumen
            worksheet.Cell(3, 1).Value = $"Total Usuarios: {datos.TotalUsuarios} | Total Ventas: {datos.TotalVentas} | Ingresos: {datos.TotalIngresos:C} | Mejor Vendedor: {datos.MejorVendedor}";
            worksheet.Range(3, 1, 3, 17).Merge();
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Headers
            var headers = new[] { "Usuario", "Nombre Completo", "Rol", "Cant. Ventas", "Total Ventas", "% Ventas", "Ticket Prom.", "Ventas Anuladas", "% Anulación", "Descuentos", "Efectivo", "Tarjeta", "Transferencia", "Nequi", "Daviplata", "QR", "Última Venta" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(5, i + 1).Value = headers[i];
                worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#BBDEFB");
                worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int rowData = 6;
            foreach (var usuario in datos.Usuarios)
            {
                worksheet.Cell(rowData, 1).Value = usuario.NombreUsuario;
                worksheet.Cell(rowData, 2).Value = usuario.NombreCompleto;
                worksheet.Cell(rowData, 3).Value = usuario.Rol;
                worksheet.Cell(rowData, 4).Value = usuario.CantidadVentas;
                worksheet.Cell(rowData, 5).Value = usuario.TotalVentas;
                worksheet.Cell(rowData, 6).Value = usuario.PorcentajeVentas / 100;
                worksheet.Cell(rowData, 7).Value = usuario.TicketPromedio;
                worksheet.Cell(rowData, 8).Value = usuario.VentasAnuladas;
                worksheet.Cell(rowData, 9).Value = usuario.PorcentajeAnulacion / 100;
                worksheet.Cell(rowData, 10).Value = usuario.DescuentosAplicados;
                worksheet.Cell(rowData, 11).Value = usuario.VentasEfectivo;
                worksheet.Cell(rowData, 12).Value = usuario.VentasTarjeta;
                worksheet.Cell(rowData, 13).Value = usuario.VentasTransferencia;
                worksheet.Cell(rowData, 14).Value = usuario.VentasNequi;
                worksheet.Cell(rowData, 15).Value = usuario.VentasDaviplata;
                worksheet.Cell(rowData, 16).Value = usuario.VentasQR;
                worksheet.Cell(rowData, 17).Value = usuario.UltimaVenta ?? (DateTime?)null;

                // Formato
                worksheet.Cell(rowData, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(rowData, 5).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(rowData, 6).Style.NumberFormat.Format = "0.00%";
                worksheet.Cell(rowData, 7).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(rowData, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(rowData, 9).Style.NumberFormat.Format = "0.00%";
                worksheet.Cell(rowData, 10).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(rowData, 11).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(rowData, 12).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(rowData, 13).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(rowData, 14).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(rowData, 15).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(rowData, 16).Style.NumberFormat.Format = "$#,##0.00";
                if (usuario.UltimaVenta.HasValue)
                    worksheet.Cell(rowData, 17).Style.NumberFormat.Format = "dd/MM/yyyy HH:mm";

                // Resaltar al mejor vendedor
                if (rowData == 6)
                {
                    worksheet.Range(rowData, 1, rowData, 17).Style.Fill.BackgroundColor = XLColor.FromHtml("#C8E6C9");
                    worksheet.Range(rowData, 1, rowData, 17).Style.Font.Bold = true;
                }

                rowData++;
            }

            // Totales
            worksheet.Cell(rowData, 1).Value = "TOTALES";
            worksheet.Cell(rowData, 1).Style.Font.Bold = true;
            worksheet.Range(rowData, 1, rowData, 3).Merge();
            worksheet.Cell(rowData, 4).Value = datos.Usuarios.Sum(u => u.CantidadVentas);
            worksheet.Cell(rowData, 5).Value = datos.TotalIngresos;
            worksheet.Cell(rowData, 7).Value = datos.TicketPromedio;
            worksheet.Cell(rowData, 8).Value = datos.Usuarios.Sum(u => u.VentasAnuladas);
            worksheet.Cell(rowData, 10).Value = datos.Usuarios.Sum(u => u.DescuentosAplicados);
            worksheet.Cell(rowData, 11).Value = datos.Usuarios.Sum(u => u.VentasEfectivo);
            worksheet.Cell(rowData, 12).Value = datos.Usuarios.Sum(u => u.VentasTarjeta);
            worksheet.Cell(rowData, 13).Value = datos.Usuarios.Sum(u => u.VentasTransferencia);
            worksheet.Cell(rowData, 14).Value = datos.Usuarios.Sum(u => u.VentasNequi);
            worksheet.Cell(rowData, 15).Value = datos.Usuarios.Sum(u => u.VentasDaviplata);
            worksheet.Cell(rowData, 16).Value = datos.Usuarios.Sum(u => u.VentasQR);

            worksheet.Range(rowData, 1, rowData, 17).Style.Font.Bold = true;
            worksheet.Range(rowData, 1, rowData, 17).Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");

            for (int col = 5; col <= 16; col++)
            {
                if (col != 6 && col != 8 && col != 9)
                    worksheet.Cell(rowData, col).Style.NumberFormat.Format = "$#,##0.00";
            }

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportarVentasPorMetodoPagoGeneralAsync(FiltroReporteDto filtro)
        {
            var datos = await ObtenerVentasPorMetodoPagoAsync(filtro);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ventas por Método Pago");

            // Título
            worksheet.Cell(1, 1).Value = "REPORTE DE VENTAS POR MÉTODO DE PAGO";
            worksheet.Range(1, 1, 1, 4).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#00BCD4");
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;

            // Período
            worksheet.Cell(2, 1).Value = $"Período: {filtro.FechaInicio:dd/MM/yyyy} - {filtro.FechaFin:dd/MM/yyyy}";
            worksheet.Range(2, 1, 2, 4).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Headers
            var headers = new[] { "Método de Pago", "Transacciones", "Monto Total", "% del Total" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(4, i + 1).Value = headers[i];
                worksheet.Cell(4, i + 1).Style.Font.Bold = true;
                worksheet.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#B2EBF2");
                worksheet.Cell(4, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int row = 5;
            foreach (var metodo in datos)
            {
                worksheet.Cell(row, 1).Value = metodo.MetodoTexto;
                worksheet.Cell(row, 2).Value = metodo.CantidadTransacciones;
                worksheet.Cell(row, 3).Value = metodo.MontoTotal;
                worksheet.Cell(row, 4).Value = metodo.PorcentajeDelTotal / 100;

                // Formato
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "0.00%";

                row++;
            }

            // Totales
            worksheet.Cell(row, 1).Value = "TOTALES";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = datos.Sum(m => m.CantidadTransacciones);
            worksheet.Cell(row, 3).Value = datos.Sum(m => m.MontoTotal);
            worksheet.Cell(row, 4).Value = 1.00;

            worksheet.Range(row, 1, row, 4).Style.Font.Bold = true;
            worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#E0F7FA");
            worksheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 4).Style.NumberFormat.Format = "0.00%";

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportarVentasPorMetodoPagoDetalladoAsync(FiltroReporteDto filtro, MetodoPago metodo)
        {
            var datos = await ObtenerVentasPorMetodoPagoAsync(filtro);
            var metodoData = datos.FirstOrDefault(m => m.Metodo == metodo);

            if (metodoData == null)
            {
                throw new Exception("No se encontraron datos para el método de pago seleccionado");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Detalle Transacciones");

            // Título
            worksheet.Cell(1, 1).Value = $"DETALLE DE TRANSACCIONES - {metodoData.MetodoTexto.ToUpper()}";
            worksheet.Range(1, 1, 1, 7).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#00838F");
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;

            // Período
            worksheet.Cell(2, 1).Value = $"Período: {filtro.FechaInicio:dd/MM/yyyy} - {filtro.FechaFin:dd/MM/yyyy}";
            worksheet.Range(2, 1, 2, 7).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Resumen
            worksheet.Cell(3, 1).Value = $"Total Transacciones: {metodoData.CantidadTransacciones} | Total: {metodoData.MontoTotal:C}";
            worksheet.Range(3, 1, 3, 7).Merge();
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Headers
            var headers = new[] { "# Venta", "Fecha", "Cliente", "Usuario", "Monto", "Autorización", "Referencia" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(5, i + 1).Value = headers[i];
                worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#B2DFDB");
                worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Datos
            int row = 6;
            foreach (var detalle in metodoData.Detalles)
            {
                worksheet.Cell(row, 1).Value = detalle.NumeroVenta;
                worksheet.Cell(row, 2).Value = detalle.FechaVenta;
                worksheet.Cell(row, 3).Value = detalle.Cliente;
                worksheet.Cell(row, 4).Value = detalle.Usuario;
                worksheet.Cell(row, 5).Value = detalle.Monto;
                worksheet.Cell(row, 6).Value = detalle.NumeroAutorizacion ?? "";
                worksheet.Cell(row, 7).Value = detalle.Referencia ?? "";

                // Formato
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "dd/MM/yyyy HH:mm";
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";

                row++;
            }

            // Total
            worksheet.Cell(row, 1).Value = "TOTAL";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Range(row, 1, row, 4).Merge();
            worksheet.Cell(row, 5).Value = metodoData.MontoTotal;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 5).Style.Font.Bold = true;
            worksheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#E0F2F1");

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportarInventarioAsync()
        {
            var reporte = await ObtenerReporteInventarioAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Inventario");

            worksheet.Cell(1, 1).Value = "REPORTE DE INVENTARIO";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;

            // Resumen
            var row = 3;
            worksheet.Cell(row++, 1).Value = "Total Productos:";
            worksheet.Cell(row - 1, 2).Value = reporte.TotalProductos;
            worksheet.Cell(row++, 1).Value = "Con Stock:";
            worksheet.Cell(row - 1, 2).Value = reporte.ProductosConStock;
            worksheet.Cell(row++, 1).Value = "Sin Stock:";
            worksheet.Cell(row - 1, 2).Value = reporte.ProductosSinStock;
            worksheet.Cell(row++, 1).Value = "Stock Bajo:";
            worksheet.Cell(row - 1, 2).Value = reporte.ProductosStockBajo;
            worksheet.Cell(row++, 1).Value = "Valor Total:";
            worksheet.Cell(row - 1, 2).Value = reporte.ValorTotalInventario;
            worksheet.Cell(row - 1, 2).Style.NumberFormat.Format = "$#,##0.00";

            // Detalle
            row += 2;
            var headerRow = row;
            worksheet.Cell(headerRow, 1).Value = "Código";
            worksheet.Cell(headerRow, 2).Value = "Producto";
            worksheet.Cell(headerRow, 3).Value = "Categoría";
            worksheet.Cell(headerRow, 4).Value = "Stock";
            worksheet.Cell(headerRow, 5).Value = "Stock Mín";
            worksheet.Cell(headerRow, 6).Value = "Precio Compra";
            worksheet.Cell(headerRow, 7).Value = "Precio Venta";
            worksheet.Cell(headerRow, 8).Value = "Valor Inv";
            worksheet.Cell(headerRow, 9).Value = "Estado";

            var headerRange = worksheet.Range(headerRow, 1, headerRow, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            row++;
            foreach (var item in reporte.Productos)
            {
                worksheet.Cell(row, 1).Value = item.Codigo;
                worksheet.Cell(row, 2).Value = item.Nombre;
                worksheet.Cell(row, 3).Value = item.Categoria;
                worksheet.Cell(row, 4).Value = item.StockActual;
                worksheet.Cell(row, 5).Value = item.StockMinimo;
                worksheet.Cell(row, 6).Value = item.PrecioCompra;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 7).Value = item.PrecioVenta;
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 8).Value = item.ValorInventario;
                worksheet.Cell(row, 8).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 9).Value = item.EstadoStock;

                // Color según estado
                if (item.EstadoStock == "Sin stock")
                    worksheet.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.Red;
                else if (item.EstadoStock == "Crítico")
                    worksheet.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.Orange;
                else if (item.EstadoStock == "Bajo")
                    worksheet.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.Yellow;

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        #endregion

        #region HELPERS

        private string ObtenerNombreMetodoPago(MetodoPago metodo)
        {
            return metodo switch
            {
                MetodoPago.Efectivo => "Efectivo",
                MetodoPago.Tarjeta => "Tarjeta",
                MetodoPago.Transferencia => "Transferencia",
                MetodoPago.QR => "QR",
                MetodoPago.Nequi => "Nequi",
                MetodoPago.Daviplata => "Daviplata",
                _ => metodo.ToString()
            };
        }

        private string ObtenerNombreRol(TipoRol rol)
        {
            return rol switch
            {
                TipoRol.Administrador => "Administrador",
                TipoRol.Supervisor => "Supervisor",
                TipoRol.Cajero => "Cajero",
                _ => "Sin rol"
            };
        }

        #endregion
    }
}