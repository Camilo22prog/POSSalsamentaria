using POS.Application.DTOs.Customers;
using POS.Application.Interfaces;
using POS.Domain.Entities.Customers;
using POS.Domain.Interfaces.Repositories;

namespace POS.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClienteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ClienteDto> CrearAsync(CrearClienteDto dto)
        {
            // Validar que no exista el documento
            if (await _unitOfWork.Clientes.ExisteDocumentoAsync(dto.NumeroDocumento))
            {
                throw new Exception($"Ya existe un cliente con el documento {dto.NumeroDocumento}");
            }

            var cliente = new Cliente
            {
                TipoCliente = dto.TipoCliente,
                TipoDocumento = dto.TipoDocumento,
                NumeroDocumento = dto.NumeroDocumento,
                NombreCompleto = dto.NombreCompleto,
                RazonSocial = dto.RazonSocial,
                NombreComercial = dto.NombreComercial,
                Pais = dto.Pais,
                Departamento = dto.Departamento,
                Ciudad = dto.Ciudad,
                Direccion = dto.Direccion,
                Email = dto.Email,
                Telefono = dto.Telefono,
                EmailSecundario = dto.EmailSecundario,
                TelefonoSecundario = dto.TelefonoSecundario,
                ResponsabilidadFiscal = dto.ResponsabilidadFiscal,
                Regimen = dto.Regimen,
                ActividadEconomica = dto.ActividadEconomica,
                Observaciones = dto.Observaciones,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            await _unitOfWork.Clientes.AddAsync(cliente);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(cliente);
        }

        public async Task<ClienteDto> ActualizarAsync(ActualizarClienteDto dto)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(dto.Id);
            if (cliente == null)
                throw new Exception("Cliente no encontrado");

            // Validar que no exista otro cliente con el mismo documento
            if (await _unitOfWork.Clientes.ExisteDocumentoAsync(dto.NumeroDocumento, dto.Id))
            {
                throw new Exception($"Ya existe otro cliente con el documento {dto.NumeroDocumento}");
            }

            cliente.TipoCliente = dto.TipoCliente;
            cliente.TipoDocumento = dto.TipoDocumento;
            cliente.NumeroDocumento = dto.NumeroDocumento;
            cliente.NombreCompleto = dto.NombreCompleto;
            cliente.RazonSocial = dto.RazonSocial;
            cliente.NombreComercial = dto.NombreComercial;
            cliente.Pais = dto.Pais;
            cliente.Departamento = dto.Departamento;
            cliente.Ciudad = dto.Ciudad;
            cliente.Direccion = dto.Direccion;
            cliente.Email = dto.Email;
            cliente.Telefono = dto.Telefono;
            cliente.EmailSecundario = dto.EmailSecundario;
            cliente.TelefonoSecundario = dto.TelefonoSecundario;
            cliente.ResponsabilidadFiscal = dto.ResponsabilidadFiscal;
            cliente.Regimen = dto.Regimen;
            cliente.ActividadEconomica = dto.ActividadEconomica;
            cliente.Observaciones = dto.Observaciones;
            cliente.Activo = dto.Activo;
            cliente.FechaModificacion = DateTime.Now;

            _unitOfWork.Clientes.Update(cliente);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(cliente);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            if (cliente == null)
                return false;

            // Soft delete
            cliente.Activo = false;
            cliente.FechaModificacion = DateTime.Now;

            _unitOfWork.Clientes.Update(cliente);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<ClienteDto?> ObtenerPorIdAsync(int id)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
            return cliente == null ? null : MapToDto(cliente);
        }

        public async Task<ClienteDto?> ObtenerPorDocumentoAsync(string numeroDocumento)
        {
            var cliente = await _unitOfWork.Clientes.ObtenerPorDocumentoAsync(numeroDocumento);
            return cliente == null ? null : MapToDto(cliente);
        }

        public async Task<IEnumerable<ClienteDto>> ObtenerTodosAsync()
        {
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            return clientes.Select(MapToDto);
        }

        public async Task<IEnumerable<ClienteDto>> BuscarAsync(string termino)
        {
            var clientes = await _unitOfWork.Clientes.BuscarAsync(termino);
            return clientes.Select(MapToDto);
        }

        public async Task<bool> ExisteDocumentoAsync(string numeroDocumento, int? clienteIdExcluir = null)
        {
            return await _unitOfWork.Clientes.ExisteDocumentoAsync(numeroDocumento, clienteIdExcluir);
        }

        private ClienteDto MapToDto(Cliente cliente)
        {
            return new ClienteDto
            {
                Id = cliente.Id,
                TipoCliente = cliente.TipoCliente,
                TipoDocumento = cliente.TipoDocumento,
                NumeroDocumento = cliente.NumeroDocumento,
                NombreCompleto = cliente.NombreCompleto,
                RazonSocial = cliente.RazonSocial,
                NombreComercial = cliente.NombreComercial,
                Pais = cliente.Pais,
                Departamento = cliente.Departamento,
                Ciudad = cliente.Ciudad,
                Direccion = cliente.Direccion,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                EmailSecundario = cliente.EmailSecundario,
                TelefonoSecundario = cliente.TelefonoSecundario,
                ResponsabilidadFiscal = cliente.ResponsabilidadFiscal,
                Regimen = cliente.Regimen,
                ActividadEconomica = cliente.ActividadEconomica,
                Observaciones = cliente.Observaciones,
                Activo = cliente.Activo,
                FechaCreacion = cliente.FechaCreacion,
                TotalCompras = cliente.Ventas.Count,
                MontoTotalCompras = cliente.Ventas.Sum(v => v.Total)
            };
        }
    }
}