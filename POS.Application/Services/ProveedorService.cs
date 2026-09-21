using POS.Application.DTOs.Catalog;
using POS.Application.Interfaces;
using POS.Domain.Entities.Purchases;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;

namespace POS.Application.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProveedorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProveedorDto>> ObtenerTodosAsync()
        {
            var lista = await _unitOfWork.Proveedores.GetAllAsync();
            return lista.Select(MapToDto);
        }

        public async Task<IEnumerable<ProveedorDto>> ObtenerActivosAsync()
        {
            var lista = await _unitOfWork.Proveedores.GetProveedoresActivosAsync();
            return lista.Select(MapToDto);
        }

        public async Task<ProveedorDto?> ObtenerPorIdAsync(int id)
        {
            var p = await _unitOfWork.Proveedores.GetByIdAsync(id);
            return p != null ? MapToDto(p) : null;
        }

        public async Task<ProveedorDto> CrearAsync(CrearProveedorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new Exception("El nombre es requerido.");

            if (await _unitOfWork.Proveedores.ExisteNombreAsync(dto.Nombre))
                throw new Exception($"Ya existe un proveedor con el nombre '{dto.Nombre}'.");

            var proveedor = new Proveedor
            {
                Nombre = dto.Nombre,
                NombreComercial = dto.NombreComercial,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion,
                PersonaContacto = dto.PersonaContacto,
                Estado = EstadoRegistro.Activo,
                FechaCreacion = DateTime.Now
            };

            await _unitOfWork.Proveedores.AddAsync(proveedor);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(proveedor);
        }

        public async Task<ProveedorDto> ActualizarAsync(int id, CrearProveedorDto dto)
        {
            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id)
                ?? throw new Exception("Proveedor no encontrado.");

            if (await _unitOfWork.Proveedores.ExisteNombreAsync(dto.Nombre, id))
                throw new Exception($"Ya existe otro proveedor con el nombre '{dto.Nombre}'.");

            proveedor.Nombre = dto.Nombre;
            proveedor.NombreComercial = dto.NombreComercial;
            proveedor.Telefono = dto.Telefono;
            proveedor.Email = dto.Email;
            proveedor.Direccion = dto.Direccion;
            proveedor.PersonaContacto = dto.PersonaContacto;
            proveedor.FechaModificacion = DateTime.Now;

            _unitOfWork.Proveedores.Update(proveedor);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(proveedor);
        }

        public async Task DesactivarAsync(int id)
        {
            var p = await _unitOfWork.Proveedores.GetByIdAsync(id)
                ?? throw new Exception("Proveedor no encontrado.");
            p.Estado = EstadoRegistro.Inactivo;
            p.FechaModificacion = DateTime.Now;
            _unitOfWork.Proveedores.Update(p);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ActivarAsync(int id)
        {
            var p = await _unitOfWork.Proveedores.GetByIdAsync(id)
                ?? throw new Exception("Proveedor no encontrado.");
            p.Estado = EstadoRegistro.Activo;
            p.FechaModificacion = DateTime.Now;
            _unitOfWork.Proveedores.Update(p);
            await _unitOfWork.SaveChangesAsync();
        }

        private static ProveedorDto MapToDto(Proveedor p) => new()
        {
            Id = p.Id,
            Nombre = p.Nombre,
            NombreComercial = p.NombreComercial,
            Telefono = p.Telefono,
            Email = p.Email,
            Direccion = p.Direccion,
            PersonaContacto = p.PersonaContacto,
            Estado = p.Estado
        };
    }
}
