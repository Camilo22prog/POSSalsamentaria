using POS.Application.DTOs.Sales;
using POS.Application.Interfaces;
using POS.Domain.Entities.Sales;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;

namespace POS.Application.Services
{
    public class CajaService : ICajaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CajaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CajaDto> AbrirCajaAsync(AbrirCajaDto dto, int usuarioId)
        {
            // Validar que no tenga caja abierta
            if (await _unitOfWork.Cajas.TieneCajaAbiertaAsync(usuarioId))
                throw new Exception("Ya tiene una caja abierta. Debe cerrarla antes de abrir una nueva.");

            var caja = new Caja
            {
                UsuarioId = usuarioId,
                FechaApertura = DateTime.Now,
                MontoInicial = dto.MontoInicial,
                Abierta = true
            };

            await _unitOfWork.Cajas.AddAsync(caja);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(caja);
        }

        public async Task<CajaDto> CerrarCajaAsync(int cajaId, CerrarCajaDto dto)
        {
            var caja = await _unitOfWork.Cajas.GetByIdAsync(cajaId);
            if (caja == null)
                throw new Exception("Caja no encontrada");

            if (!caja.Abierta)
                throw new Exception("La caja ya está cerrada");

            // Obtener ventas de la caja
            var ventas = await _unitOfWork.Ventas.GetVentasPorCajaAsync(cajaId);

            // Calcular totales del sistema
            decimal totalEfectivoSistema = 0;
            decimal totalTarjetaSistema = 0;
            decimal totalNequiSistema = 0;
            decimal totalDaviplataSistema = 0;
            decimal totalTransferenciaSistema = 0;
            decimal totalQRSistema = 0;

            foreach (var venta in ventas)
            {
                decimal cambioVenta = venta.Pagos.Sum(p => p.Cambio ?? 0);

                foreach (var pago in venta.Pagos)
                {
                    switch (pago.Metodo)
                    {
                        case MetodoPago.Efectivo:
                            totalEfectivoSistema += pago.Monto;
                            break;
                        case MetodoPago.Tarjeta:
                            totalTarjetaSistema += pago.Monto;
                            break;
                        case MetodoPago.Nequi:
                            totalNequiSistema += pago.Monto;
                            break;
                        case MetodoPago.Daviplata:
                            totalDaviplataSistema += pago.Monto;
                            break;
                        case MetodoPago.Transferencia:
                            totalTransferenciaSistema += pago.Monto;
                            break;
                        case MetodoPago.QR:
                            totalQRSistema += pago.Monto;
                            break;
                    }
                }

                totalEfectivoSistema -= cambioVenta;
            }

            // Actualizar caja
            caja.FechaCierre = DateTime.Now;
            caja.Abierta = false;
            
            caja.TotalEfectivo = totalEfectivoSistema;
            caja.TotalTarjeta = totalTarjetaSistema;
            caja.TotalNequi = totalNequiSistema;
            caja.TotalDaviplata = totalDaviplataSistema;
            caja.TotalTransferencia = totalTransferenciaSistema;
            caja.TotalQR = totalQRSistema;

            // Calcular monto final y diferencia
            var totalContado = dto.MontoEfectivoContado + dto.MontoTarjetaContado + 
                             dto.MontoNequiContado + dto.MontoDaviplataContado +
                             dto.MontoTransferenciaContado + dto.MontoQRContado;

            var totalSistema = caja.MontoInicial + totalEfectivoSistema + totalTarjetaSistema + 
                             totalNequiSistema + totalDaviplataSistema + 
                             totalTransferenciaSistema + totalQRSistema;

            caja.MontoFinal = totalContado;
            caja.Diferencia = totalContado - totalSistema;

            _unitOfWork.Cajas.Update(caja);
            await _unitOfWork.SaveChangesAsync();

            return await ObtenerResumenCajaAsync(cajaId);
        }

        public async Task<CajaDto?> ObtenerCajaActivaAsync()
        {
            var caja = await _unitOfWork.Cajas.GetCajaActivaAsync();
            if (caja == null)
                return null;

            return await ObtenerResumenCajaAsync(caja.Id);
        }

        public async Task<CajaDto?> ObtenerCajaAbiertaPorUsuarioAsync(int usuarioId)
        {
            var caja = await _unitOfWork.Cajas.GetCajaAbiertaAsync(usuarioId);
            if (caja == null)
                return null;

            return await ObtenerResumenCajaAsync(caja.Id);
        }

        public async Task<bool> TieneCajaAbiertaAsync(int usuarioId)
        {
            return await _unitOfWork.Cajas.TieneCajaAbiertaAsync(usuarioId);
        }

        public async Task<CajaDto> ObtenerResumenCajaAsync(int cajaId)
        {
            var caja = await _unitOfWork.Cajas.GetByIdAsync(cajaId);
            if (caja == null)
                throw new Exception("Caja no encontrada");

            var ventas = await _unitOfWork.Ventas.GetVentasPorCajaAsync(cajaId);

            var dto = MapToDto(caja);
            dto.CantidadVentas = ventas.Count();

            // Calcular totales si la caja está abierta
            if (caja.Abierta)
            {
                decimal totalEfectivo = 0;
                decimal totalTarjeta = 0;
                decimal totalNequi = 0;
                decimal totalDaviplata = 0;
                decimal totalTransferencia = 0;
                decimal totalQR = 0;

                foreach (var venta in ventas)
                {
                    decimal cambioVenta = venta.Pagos.Sum(p => p.Cambio ?? 0);

                    foreach (var pago in venta.Pagos)
                    {
                        switch (pago.Metodo)
                        {
                            case MetodoPago.Efectivo:
                                totalEfectivo += pago.Monto;
                                break;
                            case MetodoPago.Tarjeta:
                                totalTarjeta += pago.Monto;
                                break;
                            case MetodoPago.Nequi:
                                totalNequi += pago.Monto;
                                break;
                            case MetodoPago.Daviplata:
                                totalDaviplata += pago.Monto;
                                break;
                            case MetodoPago.Transferencia:
                                totalTransferencia += pago.Monto;
                                break;
                            case MetodoPago.QR:
                                totalQR += pago.Monto;
                                break;
                        }
                    }

                    totalEfectivo -= cambioVenta;
                }

                dto.TotalEfectivo = totalEfectivo;
                dto.TotalTarjeta = totalTarjeta;
                dto.TotalNequi = totalNequi;
                dto.TotalDaviplata = totalDaviplata;
                dto.TotalTransferencia = totalTransferencia;
                dto.TotalQR = totalQR;
            }

            return dto;
        }

        // ==================== NUEVOS MÉTODOS PARA ARQUEO ====================

        public async Task<CajaDto> AbrirCajaConDenominacionesAsync(AbrirCajaConDenominacionesDto dto, int usuarioId)
        {
            // Validar que no tenga caja abierta
            if (await _unitOfWork.Cajas.TieneCajaAbiertaAsync(usuarioId))
                throw new Exception("Ya tiene una caja abierta. Debe cerrarla antes de abrir una nueva.");

            // Calcular monto inicial
            var montoInicial = dto.Monedas.Sum(m => m.Total) + dto.Billetes.Sum(b => b.Total);

            var caja = new Caja
            {
                UsuarioId = usuarioId,
                FechaApertura = DateTime.Now,
                MontoInicial = montoInicial,
                Abierta = true,
                EfectivoEsperado = montoInicial,
                TotalRetiros = 0
            };

            await _unitOfWork.Cajas.AddAsync(caja);
            await _unitOfWork.SaveChangesAsync();

            // Guardar detalles de denominaciones
            foreach (var moneda in dto.Monedas)
            {
                var detalle = new DetalleDenominacion
                {
                    CajaId = caja.Id,
                    Tipo = TipoDenominacion.Moneda,
                    Valor = moneda.Valor,
                    Cantidad = moneda.Cantidad,
                    Total = moneda.Total,
                    TipoArqueo = TipoArqueo.Apertura,
                    Fecha = DateTime.Now
                };
                await _unitOfWork.DetallesDenominaciones.AddAsync(detalle);
            }

            foreach (var billete in dto.Billetes)
            {
                var detalle = new DetalleDenominacion
                {
                    CajaId = caja.Id,
                    Tipo = TipoDenominacion.Billete,
                    Valor = billete.Valor,
                    Cantidad = billete.Cantidad,
                    Total = billete.Total,
                    TipoArqueo = TipoArqueo.Apertura,
                    Fecha = DateTime.Now
                };
                await _unitOfWork.DetallesDenominaciones.AddAsync(detalle);
            }

            await _unitOfWork.SaveChangesAsync();

            return MapToDto(caja);
        }

        public async Task<CajaDto> CerrarCajaConArqueoAsync(int cajaId, CerrarCajaConArqueoDto dto)
        {
            var caja = await _unitOfWork.Cajas.GetByIdAsync(cajaId);
            if (caja == null)
                throw new Exception("Caja no encontrada");

            if (!caja.Abierta)
                throw new Exception("La caja ya está cerrada");

            // Obtener ventas y retiros de la caja
            var ventas = await _unitOfWork.Ventas.GetVentasPorCajaAsync(cajaId);
            var retiros = await _unitOfWork.RetirosCaja.ObtenerPorCajaAsync(cajaId);

            // Calcular totales del sistema por método de pago
            decimal totalEfectivoVentas = 0;
            decimal totalTarjeta = 0;
            decimal totalNequi = 0;
            decimal totalDaviplata = 0;
            decimal totalTransferencia = 0;
            decimal totalQR = 0;

            foreach (var venta in ventas)
            {
                decimal cambioVenta = venta.Pagos.Sum(p => p.Cambio ?? 0);

                foreach (var pago in venta.Pagos)
                {
                    switch (pago.Metodo)
                    {
                        case MetodoPago.Efectivo:
                            totalEfectivoVentas += pago.Monto;
                            break;
                        case MetodoPago.Tarjeta:
                            totalTarjeta += pago.Monto;
                            break;
                        case MetodoPago.Nequi:
                            totalNequi += pago.Monto;
                            break;
                        case MetodoPago.Daviplata:
                            totalDaviplata += pago.Monto;
                            break;
                        case MetodoPago.Transferencia:
                            totalTransferencia += pago.Monto;
                            break;
                        case MetodoPago.QR:
                            totalQR += pago.Monto;
                            break;
                    }
                }

                totalEfectivoVentas -= cambioVenta;
            }

            // Calcular total de retiros
            var totalRetiros = retiros.Sum(r => r.Monto);

            // Calcular efectivo esperado
            var efectivoEsperado = caja.MontoInicial + totalEfectivoVentas - totalRetiros;

            // Calcular efectivo contado
            var efectivoContado = dto.Monedas.Sum(m => m.Total) + dto.Billetes.Sum(b => b.Total);

            // Actualizar caja
            caja.FechaCierre = DateTime.Now;
            caja.Abierta = false;
            
            caja.TotalEfectivo = totalEfectivoVentas;
            caja.TotalTarjeta = totalTarjeta;
            caja.TotalNequi = totalNequi;
            caja.TotalDaviplata = totalDaviplata;
            caja.TotalTransferencia = totalTransferencia;
            caja.TotalQR = totalQR;
            caja.TotalRetiros = totalRetiros;

            caja.EfectivoEsperado = efectivoEsperado;
            caja.EfectivoContado = efectivoContado;
            caja.DiferenciaEfectivo = efectivoContado - efectivoEsperado;

            var totalSistema = efectivoEsperado + totalTarjeta + totalNequi + 
                              totalDaviplata + totalTransferencia + totalQR;
            
            var totalContado = efectivoContado + dto.MontoTarjetaContado + dto.MontoNequiContado +
                              dto.MontoDaviplataContado + dto.MontoTransferenciaContado + dto.MontoQRContado;

            caja.MontoFinal = totalContado;
            caja.Diferencia = totalContado - totalSistema;

            _unitOfWork.Cajas.Update(caja);

            // Guardar detalles de denominaciones del cierre
            foreach (var moneda in dto.Monedas)
            {
                var detalle = new DetalleDenominacion
                {
                    CajaId = caja.Id,
                    Tipo = TipoDenominacion.Moneda,
                    Valor = moneda.Valor,
                    Cantidad = moneda.Cantidad,
                    Total = moneda.Total,
                    TipoArqueo = TipoArqueo.Cierre,
                    Fecha = DateTime.Now
                };
                await _unitOfWork.DetallesDenominaciones.AddAsync(detalle);
            }

            foreach (var billete in dto.Billetes)
            {
                var detalle = new DetalleDenominacion
                {
                    CajaId = caja.Id,
                    Tipo = TipoDenominacion.Billete,
                    Valor = billete.Valor,
                    Cantidad = billete.Cantidad,
                    Total = billete.Total,
                    TipoArqueo = TipoArqueo.Cierre,
                    Fecha = DateTime.Now
                };
                await _unitOfWork.DetallesDenominaciones.AddAsync(detalle);
            }

            await _unitOfWork.SaveChangesAsync();

            return await ObtenerResumenCajaAsync(cajaId);
        }

        public async Task<RetiroCajaDto> RegistrarRetiroAsync(int cajaId, CrearRetiroDto dto, int usuarioId)
        {
            var caja = await _unitOfWork.Cajas.GetByIdAsync(cajaId);
            if (caja == null)
                throw new Exception("Caja no encontrada");

            if (!caja.Abierta)
                throw new Exception("La caja está cerrada. No se pueden registrar retiros.");

            var retiro = new RetiroCaja
            {
                CajaId = cajaId,
                Monto = dto.Monto,
                Motivo = dto.Motivo,
                Observaciones = dto.Observaciones,
                UsuarioId = usuarioId,
                Fecha = DateTime.Now
            };

            await _unitOfWork.RetirosCaja.AddAsync(retiro);
            
            // Actualizar total de retiros en la caja
            caja.TotalRetiros += dto.Monto;
            caja.EfectivoEsperado = caja.MontoInicial + caja.TotalEfectivo - caja.TotalRetiros;
            
            _unitOfWork.Cajas.Update(caja);
            await _unitOfWork.SaveChangesAsync();

            return new RetiroCajaDto
            {
                Id = retiro.Id,
                CajaId = retiro.CajaId,
                Monto = retiro.Monto,
                Motivo = retiro.Motivo,
                Observaciones = retiro.Observaciones,
                UsuarioId = retiro.UsuarioId,
                Fecha = retiro.Fecha
            };
        }

        public async Task<List<RetiroCajaDto>> ObtenerRetirosCajaAsync(int cajaId)
        {
            var retiros = await _unitOfWork.RetirosCaja.ObtenerPorCajaAsync(cajaId);

            return retiros.Select(r => new RetiroCajaDto
            {
                Id = r.Id,
                CajaId = r.CajaId,
                Monto = r.Monto,
                Motivo = r.Motivo,
                Observaciones = r.Observaciones,
                UsuarioId = r.UsuarioId,
                Fecha = r.Fecha
            }).ToList();
        }

        public async Task<List<DetalleDenominacionDto>> ObtenerDenominacionesAperturaAsync(int cajaId)
        {
            var detalles = await _unitOfWork.DetallesDenominaciones.ObtenerPorCajaYTipoAsync(cajaId, TipoArqueo.Apertura);

            return detalles.Select(d => new DetalleDenominacionDto
            {
                Valor = d.Valor,
                Cantidad = d.Cantidad,
                Total = d.Total
            }).ToList();
        }

        // ==================== MAPEO ====================

        private CajaDto MapToDto(Caja caja)
        {
            return new CajaDto
            {
                Id = caja.Id,
                UsuarioId = caja.UsuarioId,
                UsuarioNombre = "",
                FechaApertura = caja.FechaApertura,
                FechaCierre = caja.FechaCierre,
                MontoInicial = caja.MontoInicial,
                MontoFinal = caja.MontoFinal,
                Diferencia = caja.Diferencia,
                Abierta = caja.Abierta,
                TotalEfectivo = caja.TotalEfectivo,
                TotalTarjeta = caja.TotalTarjeta,
                TotalNequi = caja.TotalNequi,
                TotalDaviplata = caja.TotalDaviplata,
                TotalTransferencia = caja.TotalTransferencia,
                TotalQR = caja.TotalQR,
                TotalRetiros = caja.TotalRetiros,
                EfectivoEsperado = caja.EfectivoEsperado,
                EfectivoContado = caja.EfectivoContado,
                DiferenciaEfectivo = caja.DiferenciaEfectivo
            };
        }
    }
}