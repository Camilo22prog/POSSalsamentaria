using POS.Application.DTOs.Expenses;
using POS.Application.Interfaces;
using POS.Domain.Entities.Expenses;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;

namespace POS.Application.Services
{
    public class GastoService : IGastoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GastoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<GastoOperativoDto>> ObtenerTodosAsync()
        {
            var lista = await _unitOfWork.Gastos.GetAllAsync();
            return lista.OrderByDescending(g => g.Fecha).Select(MapToDto);
        }

        public async Task<IEnumerable<GastoOperativoDto>> ObtenerPorFechaAsync(DateTime inicio, DateTime fin)
        {
            var finDia = fin.Date.AddDays(1).AddTicks(-1);
            var lista = await _unitOfWork.Gastos.FindAsync(
                g => g.Fecha >= inicio.Date && g.Fecha <= finDia);
            return lista.OrderByDescending(g => g.Fecha).Select(MapToDto);
        }

        public async Task<IEnumerable<GastoOperativoDto>> ObtenerPorTipoAsync(TipoGasto tipo)
        {
            var lista = await _unitOfWork.Gastos.FindAsync(g => g.Tipo == tipo);
            return lista.OrderByDescending(g => g.Fecha).Select(MapToDto);
        }

        public async Task<GastoOperativoDto?> ObtenerPorIdAsync(int id)
        {
            var g = await _unitOfWork.Gastos.GetByIdAsync(id);
            return g != null ? MapToDto(g) : null;
        }

        public async Task<GastoOperativoDto> CrearAsync(CrearGastoOperativoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Descripcion))
                throw new Exception("La descripción es requerida.");
            if (dto.Monto <= 0)
                throw new Exception("El monto debe ser mayor a cero.");

            var gasto = new GastoOperativo
            {
                Tipo         = dto.Tipo,
                Descripcion  = dto.Descripcion.Trim(),
                Monto        = dto.Monto,
                Fecha        = dto.Fecha.Date,
                Comprobante  = string.IsNullOrWhiteSpace(dto.Comprobante) ? null : dto.Comprobante.Trim(),
                Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim(),
                UsuarioId    = dto.UsuarioId,
                FechaCreacion = DateTime.Now
            };

            await _unitOfWork.Gastos.AddAsync(gasto);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(gasto);
        }

        public async Task<GastoOperativoDto> ActualizarAsync(int id, CrearGastoOperativoDto dto)
        {
            var gasto = await _unitOfWork.Gastos.GetByIdAsync(id)
                ?? throw new Exception("Gasto no encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Descripcion))
                throw new Exception("La descripción es requerida.");
            if (dto.Monto <= 0)
                throw new Exception("El monto debe ser mayor a cero.");

            gasto.Tipo        = dto.Tipo;
            gasto.Descripcion = dto.Descripcion.Trim();
            gasto.Monto       = dto.Monto;
            gasto.Fecha       = dto.Fecha.Date;
            gasto.Comprobante  = string.IsNullOrWhiteSpace(dto.Comprobante) ? null : dto.Comprobante.Trim();
            gasto.Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim();

            _unitOfWork.Gastos.Update(gasto);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(gasto);
        }

        public async Task EliminarAsync(int id)
        {
            var gasto = await _unitOfWork.Gastos.GetByIdAsync(id)
                ?? throw new Exception("Gasto no encontrado.");
            _unitOfWork.Gastos.Remove(gasto);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<decimal> ObtenerTotalPorFechaAsync(DateTime inicio, DateTime fin)
        {
            var finDia = fin.Date.AddDays(1).AddTicks(-1);
            var lista = await _unitOfWork.Gastos.FindAsync(
                g => g.Fecha >= inicio.Date && g.Fecha <= finDia);
            return lista.Sum(g => g.Monto);
        }

        private static GastoOperativoDto MapToDto(GastoOperativo g) => new()
        {
            Id           = g.Id,
            Tipo         = g.Tipo,
            TipoNombre   = ObtenerNombreTipo(g.Tipo),
            Descripcion  = g.Descripcion,
            Monto        = g.Monto,
            Fecha        = g.Fecha,
            Comprobante  = g.Comprobante,
            Observaciones = g.Observaciones,
            UsuarioNombre = g.Usuario?.NombreCompleto ?? string.Empty
        };

        private static string ObtenerNombreTipo(TipoGasto tipo) => tipo switch
        {
            TipoGasto.Arriendo        => "Arriendo",
            TipoGasto.Servicios       => "Servicios (Agua/Luz/Internet)",
            TipoGasto.Nomina          => "Nómina / Sueldos",
            TipoGasto.Mantenimiento   => "Mantenimiento",
            TipoGasto.Papeleria       => "Papelería / Insumos",
            TipoGasto.Publicidad      => "Publicidad / Marketing",
            TipoGasto.Transporte      => "Transporte / Fletes",
            TipoGasto.ImpuestosYTasas => "Impuestos y Tasas",
            TipoGasto.Seguros         => "Seguros",
            TipoGasto.Otro            => "Otro",
            _                         => tipo.ToString()
        };
    }
}
