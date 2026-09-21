using POS.Application.DTOs.Catalog;
using POS.Application.Interfaces;
using POS.Domain.Entities.Catalog;
using POS.Domain.Enums;
using POS.Domain.Interfaces.Repositories;

namespace POS.Application.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProductoDto>> ObtenerTodosAsync()
        {
            var productos = await _unitOfWork.Productos.GetAllAsync();
            var productosConCategoria = new List<Producto>();
            
            foreach (var p in productos)
            {
                var producto = await _unitOfWork.Productos.GetProductoConCategoriaAsync(p.Id);
                if (producto != null)
                    productosConCategoria.Add(producto);
            }
            
            return productosConCategoria.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductoDto>> ObtenerActivosAsync()
        {
            var productos = await _unitOfWork.Productos.GetProductosActivosAsync();
            return productos.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductoDto>> ObtenerPorCategoriaAsync(int categoriaId)
        {
            var productos = await _unitOfWork.Productos.GetProductosPorCategoriaAsync(categoriaId);
            return productos.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductoDto>> ObtenerPorProveedorAsync(int proveedorId)
        {
            var productos = await _unitOfWork.Productos.GetProductosPorProveedorAsync(proveedorId);
            return productos.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductoDto>> ObtenerConStockBajoAsync()
        {
            var productos = await _unitOfWork.Productos.GetProductosConStockBajoAsync();
            return productos.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductoDto>> BuscarAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await ObtenerActivosAsync();

            var productos = await _unitOfWork.Productos.BuscarProductosAsync(termino);
            return productos.Select(MapToDto);
        }

        public async Task<ProductoDto?> ObtenerPorIdAsync(int id)
        {
            var producto = await _unitOfWork.Productos.GetProductoConCategoriaAsync(id);
            return producto != null ? MapToDto(producto) : null;
        }

        public async Task<ProductoDto?> ObtenerPorCodigoAsync(string codigo)
        {
            var producto = await _unitOfWork.Productos.GetByCodigoAsync(codigo);
            return producto != null ? MapToDto(producto) : null;
        }

        public async Task<ProductoDto> CrearAsync(CrearProductoDto dto, int creadoPorId)
        {
            // Validaciones
            if (await _unitOfWork.Productos.ExisteCodigoAsync(dto.Codigo))
                throw new Exception($"Ya existe un producto con el código '{dto.Codigo}'");

            var categoria = await _unitOfWork.Categorias.GetByIdAsync(dto.CategoriaId);
            if (categoria == null)
                throw new Exception("Categoría no encontrada");

            if (dto.PrecioVenta <= 0)
                throw new Exception("El precio de venta debe ser mayor a cero");

            var producto = new Producto
            {
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CategoriaId = dto.CategoriaId,
                TipoUnidad = dto.TipoUnidad,
                PrecioCompra = dto.PrecioCompra,
                PrecioVenta = dto.PrecioVenta,
                TipoIVA = dto.TipoIVA,
                StockActual = 0,
                StockMinimo = dto.StockMinimo,
                UnidadMedida = dto.UnidadMedida,
                ControlaVencimiento = dto.ControlaVencimiento,
                DiasAlertaVencimiento = dto.DiasAlertaVencimiento,
                PermiteFraccionado = dto.PermiteFraccionado,
                ImagenUrl = dto.ImagenUrl,
                VentaPorPeso = dto.VentaPorPeso,
                CategoriaPeso = dto.CategoriaPeso,
                PrecioPorKilo = dto.PrecioPorKilo,
                ProveedorId = dto.ProveedorId,
                Estado = EstadoRegistro.Activo,
                FechaCreacion = DateTime.Now
            };

            await _unitOfWork.Productos.AddAsync(producto);
            await _unitOfWork.SaveChangesAsync();

            // Recargar con categoría
            var productoCreado = await _unitOfWork.Productos.GetProductoConCategoriaAsync(producto.Id);
            return MapToDto(productoCreado!);
        }

        public async Task<ProductoDto> ActualizarAsync(int id, CrearProductoDto dto, int modificadoPorId)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (producto == null)
                throw new Exception("Producto no encontrado");

            // Validar código único (excepto el actual)
            if (await _unitOfWork.Productos.ExisteCodigoAsync(dto.Codigo, id))
                throw new Exception($"Ya existe otro producto con el código '{dto.Codigo}'");

            var categoria = await _unitOfWork.Categorias.GetByIdAsync(dto.CategoriaId);
            if (categoria == null)
                throw new Exception("Categoría no encontrada");

            if (dto.PrecioVenta <= 0)
                throw new Exception("El precio de venta debe ser mayor a cero");

            producto.Codigo = dto.Codigo;
            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.CategoriaId = dto.CategoriaId;
            producto.TipoUnidad = dto.TipoUnidad;
            producto.PrecioCompra = dto.PrecioCompra;
            producto.PrecioVenta = dto.PrecioVenta;
            producto.TipoIVA = dto.TipoIVA;
            producto.StockMinimo = dto.StockMinimo;
            producto.UnidadMedida = dto.UnidadMedida;
            producto.ControlaVencimiento = dto.ControlaVencimiento;
            producto.DiasAlertaVencimiento = dto.DiasAlertaVencimiento;
            producto.PermiteFraccionado = dto.PermiteFraccionado;
            producto.ImagenUrl = dto.ImagenUrl;
            producto.VentaPorPeso = dto.VentaPorPeso;
            producto.CategoriaPeso = dto.CategoriaPeso;
            producto.PrecioPorKilo = dto.PrecioPorKilo;
            producto.ProveedorId = dto.ProveedorId;
            producto.FechaModificacion = DateTime.Now;

            _unitOfWork.Productos.Update(producto);
            await _unitOfWork.SaveChangesAsync();

            // Recargar con categoría
            var productoActualizado = await _unitOfWork.Productos.GetProductoConCategoriaAsync(id);
            return MapToDto(productoActualizado!);
        }

        public async Task DesactivarAsync(int id, int modificadoPorId)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (producto == null)
                throw new Exception("Producto no encontrado");

            producto.Estado = EstadoRegistro.Inactivo;
            producto.FechaModificacion = DateTime.Now;

            _unitOfWork.Productos.Update(producto);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ActivarAsync(int id, int modificadoPorId)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (producto == null)
                throw new Exception("Producto no encontrado");

            producto.Estado = EstadoRegistro.Activo;
            producto.FechaModificacion = DateTime.Now;

            _unitOfWork.Productos.Update(producto);
            await _unitOfWork.SaveChangesAsync();
        }

        private ProductoDto MapToDto(Producto producto)
        {
            return new ProductoDto
            {
                Id = producto.Id,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                CategoriaId = producto.CategoriaId,
                CategoriaNombre = producto.Categoria?.Nombre ?? "",
                CategoriaColor = producto.Categoria?.Color,
                TipoUnidad = producto.TipoUnidad,
                TipoUnidadTexto = ObtenerTextoTipoUnidad(producto.TipoUnidad),
                PrecioCompra = producto.PrecioCompra,
                PrecioVenta = producto.PrecioVenta,
                TipoIVA = producto.TipoIVA,
                TipoIVATexto = ObtenerTextoTipoIVA(producto.TipoIVA),
                PorcentajeIVA = (decimal)producto.TipoIVA,
                StockActual = producto.StockActual,
                StockMinimo = producto.StockMinimo,
                UnidadMedida = producto.UnidadMedida,
                TieneStockBajo = producto.StockActual <= producto.StockMinimo,
                ControlaVencimiento = producto.ControlaVencimiento,
                PermiteFraccionado = producto.PermiteFraccionado,
                Estado = producto.Estado,
                ImagenUrl = producto.ImagenUrl,
                
                // ✅ MAPEO DE CAMPOS DE VENTA POR PESO
                VentaPorPeso = producto.VentaPorPeso,
                CategoriaPeso = producto.CategoriaPeso,
                PrecioPorKilo = producto.PrecioPorKilo,

                // Proveedor
                ProveedorId = producto.ProveedorId,
                ProveedorNombre = producto.Proveedor?.Nombre
            };
        }

        private string ObtenerTextoTipoUnidad(TipoUnidad tipo)
        {
            return tipo switch
            {
                TipoUnidad.Unidad => "Unidad",
                TipoUnidad.Peso => "Peso",
                TipoUnidad.Ambos => "Unidad/Peso",
                _ => ""
            };
        }

        private string ObtenerTextoTipoIVA(TipoIVA tipo)
        {
            return tipo switch
            {
                TipoIVA.Exento => "Exento (0%)",
                TipoIVA.Cinco => "5%",
                TipoIVA.Diecinueve => "19%",
                _ => ""
            };
        }
    }
}