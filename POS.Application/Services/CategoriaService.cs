using POS.Application.DTOs.Catalog;
using POS.Application.Interfaces;
using POS.Domain.Entities.Catalog;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;

namespace POS.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoriaDto>> ObtenerTodasAsync()
        {
            var categorias = await _unitOfWork.Categorias.GetAllAsync();
            return categorias.Select(MapToDto);
        }

        public async Task<IEnumerable<CategoriaDto>> ObtenerActivasAsync()
        {
            var categorias = await _unitOfWork.Categorias.GetCategoriasActivasAsync();
            return categorias.Select(MapToDto);
        }

        public async Task<CategoriaDto?> ObtenerPorIdAsync(int id)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(id);
            return categoria != null ? MapToDto(categoria) : null;
        }

        public async Task<CategoriaDto> CrearAsync(CrearCategoriaDto dto, int creadoPorId)
        {
            // Validar que no exista el nombre
            if (await _unitOfWork.Categorias.ExisteNombreAsync(dto.Nombre))
                throw new Exception($"Ya existe una categoría con el nombre '{dto.Nombre}'");

            var categoria = new Categoria
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Color = dto.Color ?? "#673AB7",
                Estado = EstadoRegistro.Activo,
                FechaCreacion = DateTime.Now
            };

            await _unitOfWork.Categorias.AddAsync(categoria);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(categoria);
        }

        public async Task<CategoriaDto> ActualizarAsync(int id, CrearCategoriaDto dto, int modificadoPorId)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(id);
            if (categoria == null)
                throw new Exception("Categoría no encontrada");

            // Validar que no exista el nombre (excepto la actual)
            if (await _unitOfWork.Categorias.ExisteNombreAsync(dto.Nombre, id))
                throw new Exception($"Ya existe otra categoría con el nombre '{dto.Nombre}'");

            categoria.Nombre = dto.Nombre;
            categoria.Descripcion = dto.Descripcion;
            categoria.Color = dto.Color ?? categoria.Color;
            categoria.FechaModificacion = DateTime.Now;

            _unitOfWork.Categorias.Update(categoria);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(categoria);
        }

        public async Task DesactivarAsync(int id, int modificadoPorId)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(id);
            if (categoria == null)
                throw new Exception("Categoría no encontrada");

            categoria.Estado = EstadoRegistro.Inactivo;
            categoria.FechaModificacion = DateTime.Now;

            _unitOfWork.Categorias.Update(categoria);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ActivarAsync(int id, int modificadoPorId)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(id);
            if (categoria == null)
                throw new Exception("Categoría no encontrada");

            categoria.Estado = EstadoRegistro.Activo;
            categoria.FechaModificacion = DateTime.Now;

            _unitOfWork.Categorias.Update(categoria);
            await _unitOfWork.SaveChangesAsync();
        }

        private CategoriaDto MapToDto(Categoria categoria)
        {
            return new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Color = categoria.Color,
                Estado = categoria.Estado
            };
        }
    }
}