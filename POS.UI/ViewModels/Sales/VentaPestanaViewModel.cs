using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using POS.Application.DTOs.Catalog;
using POS.Application.DTOs.Customers;  // ✅ AGREGAR
using POS.Application.Interfaces;
using POS.UI.Models;
using POS.UI.Views.Catalog;
using POS.UI.Views.Customers;  // ✅ AGREGAR
using POS.UI.Views.Sales;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using POS.UI.ViewModels.Catalog;
using POS.UI.ViewModels.Customers;  // ✅ AGREGAR

namespace POS.UI.ViewModels.Sales
{
    public partial class VentaPestanaViewModel : ObservableObject
    {
        private readonly IProductoService _productoService;
        private readonly IClienteService _clienteService;  // ✅ AGREGAR
        private readonly Action<VentaPestanaViewModel> _onCerrarPestana;
        private readonly int _cajaId;

        [ObservableProperty]
        private string _titulo = "Nueva Venta";

        [ObservableProperty]
        private string _codigoBusqueda = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ItemCarrito> _items = new();

        [ObservableProperty]
        private ItemCarrito? _itemSeleccionado;

        [ObservableProperty]
        private decimal _subtotal;

        [ObservableProperty]
        private decimal _descuentoTotal;

        [ObservableProperty]
        private decimal _ivaTotal;

        [ObservableProperty]
        private decimal _total;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        // Para búsqueda de productos con preview
        [ObservableProperty]
        private ObservableCollection<ProductoDto> _resultadosBusqueda = new();

        [ObservableProperty]
        private bool _mostrarResultados;

        [ObservableProperty]
        private bool _productoNoEncontrado;

        [ObservableProperty]
        private bool _carritoVacio = true;

        // ✅ PROPIEDADES PARA CLIENTES
        [ObservableProperty]
        private ClienteDto? _clienteSeleccionado;

        [ObservableProperty]
        private string _busquedaCliente = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ClienteDto> _clientesEncontrados = new();

        [ObservableProperty]
        private bool _mostrarResultadosClientes;

        public int NumeroPestana { get; set; }

        public VentaPestanaViewModel(
            IProductoService productoService,
            IClienteService clienteService,
            int numeroPestana,
            int cajaId,
            Action<VentaPestanaViewModel> onCerrarPestana)
        {
            _productoService = productoService;
            _clienteService = clienteService;
            NumeroPestana = numeroPestana;
            _cajaId = cajaId;
            _onCerrarPestana = onCerrarPestana;

            Titulo = $"Venta #{numeroPestana}";
        }

        // ✅ MÉTODOS PARA CLIENTES
        [RelayCommand]
        private async Task BuscarClienteAsync()
        {
            if (string.IsNullOrWhiteSpace(BusquedaCliente) || BusquedaCliente.Length < 2)
            {
                ClientesEncontrados.Clear();
                MostrarResultadosClientes = false;
                return;
            }

            try
            {
                var clientes = await _clienteService.BuscarAsync(BusquedaCliente);
                ClientesEncontrados = new ObservableCollection<ClienteDto>(clientes.Take(10));
                MostrarResultadosClientes = ClientesEncontrados.Any();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar clientes: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SeleccionarCliente(ClienteDto cliente)
        {
            ClienteSeleccionado = cliente;
            BusquedaCliente = cliente.NombreCompleto;
            MostrarResultadosClientes = false;
            ClientesEncontrados.Clear();
        }

        [RelayCommand]
        private void LimpiarCliente()
        {
            ClienteSeleccionado = null;
            BusquedaCliente = string.Empty;
            ClientesEncontrados.Clear();
            MostrarResultadosClientes = false;
        }

        [RelayCommand]
        private void AbrirNuevoCliente()
        {
            try
            {
                var clienteFormViewModel = new ClienteFormViewModel(_clienteService);
                var clienteFormView = new ClienteFormView(clienteFormViewModel);
                
                if (clienteFormView.ShowDialog() == true)
                {
                    // Recargar después de crear
                    _ = BuscarClienteAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir formulario: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Búsqueda en tiempo real mientras escribe
        partial void OnCodigoBusquedaChanged(string value)
        {
            ProductoNoEncontrado = false;
            MensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                ResultadosBusqueda.Clear();
                MostrarResultados = false;
                return;
            }

            // Buscar en tiempo real
            _ = BuscarProductosEnTiempoRealAsync(value);
        }

        // ✅ AGREGAR ESTE MÉTODO PARA BÚSQUEDA DE CLIENTES EN TIEMPO REAL
        partial void OnBusquedaClienteChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
            {
                ClientesEncontrados.Clear();
                MostrarResultadosClientes = false;
                return;
            }

            _ = BuscarClienteAsync();
        }

        private async Task BuscarProductosEnTiempoRealAsync(string termino)
        {
            try
            {
                var productos = await _productoService.BuscarAsync(termino);
                var lista = productos.ToList();

                ResultadosBusqueda.Clear();

                if (lista.Any())
                {
                    foreach (var p in lista.Take(8)) // máximo 8 sugerencias
                    {
                        ResultadosBusqueda.Add(p);
                    }
                    MostrarResultados = true;
                    ProductoNoEncontrado = false;
                }
                else
                {
                    MostrarResultados = false;
                    // Solo mostrar "no encontrado" si el término tiene más de 2 caracteres
                    if (termino.Length > 2)
                    {
                        ProductoNoEncontrado = true;
                    }
                }
            }
            catch { }
        }

        [RelayCommand]
        private async Task BuscarYAgregarProductoAsync()
        {
            if (string.IsNullOrWhiteSpace(CodigoBusqueda))
                return;

            IsLoading = true;
            MensajeError = string.Empty;
            ProductoNoEncontrado = false;

            try
            {
                var termino = CodigoBusqueda.Trim();

                // 1. Intentar buscar por código exacto primero (ideal para lector de barras o códigos directos)
                var productoExacto = await _productoService.ObtenerPorCodigoAsync(termino);
                if (productoExacto != null && productoExacto.Estado == POS.Domain.Enums.EstadoRegistro.Activo)
                {
                    AgregarProductoAlCarrito(productoExacto);
                    return;
                }

                // 2. Si no hay coincidencia exacta de código, hacer la búsqueda general
                var productos = await _productoService.BuscarAsync(termino);
                var lista = productos.ToList();

                if (!lista.Any())
                {
                    // Producto no encontrado - mostrar diálogo con opciones
                    System.Media.SystemSounds.Exclamation.Play();
                    
                    var resultado = MessageBox.Show(
                        $"⚠️ Producto '{termino}' no encontrado en el sistema.\n\n" +
                        $"¿Desea crear este producto ahora?\n\n" +
                        $"• SÍ → Abrir formulario para crear el producto\n" +
                        $"• NO → Continuar con la venta",
                        "Producto No Encontrado",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Abrir formulario de nuevo producto
                            var productoFormViewModel = new ProductoFormViewModel(
                                App.Services.GetRequiredService<IProductoService>(),
                                App.Services.GetRequiredService<ICategoriaService>(),
                                App.Services.GetRequiredService<IProveedorService>(),
                                () => { }); // Callback vacío
                            
                            // Cargar categorías
                            await productoFormViewModel.CargarCategoriasAsync();
                            
                            // Pre-cargar el código buscado
                            productoFormViewModel.Codigo = termino;
                            productoFormViewModel.Titulo = "Crear Nuevo Producto";
                            
                            var formWindow = new ProductoFormView(productoFormViewModel);
                            var dialogResult = formWindow.ShowDialog();

                            // Si se guardó el producto, buscarlo y agregarlo
                            if (dialogResult == true && productoFormViewModel.ProductoGuardado)
                            {
                                await Task.Delay(300); // Pequeña espera para que se guarde en BD
                                var productosNuevos = await _productoService.BuscarAsync(termino);
                                var productoNuevo = productosNuevos.FirstOrDefault();
                                if (productoNuevo != null)
                                {
                                    AgregarProductoAlCarrito(productoNuevo);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Error al abrir formulario: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }

                    CodigoBusqueda = string.Empty;
                    MostrarResultados = false;
                    return;
                }

                if (lista.Count == 1)
                {
                    AgregarProductoAlCarrito(lista.First());
                    return;
                }

                // Múltiples resultados - mostrar lista
                ResultadosBusqueda.Clear();
                foreach (var p in lista.Take(8))
                    ResultadosBusqueda.Add(p);
                MostrarResultados = true;
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al buscar producto: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }


        [RelayCommand]
        private void SeleccionarProductoDeLista(ProductoDto producto)
        {
            if (producto == null) return;

            AgregarProductoAlCarrito(producto);
            MostrarResultados = false;
            ResultadosBusqueda.Clear();
            CodigoBusqueda = string.Empty;
        }

        private void AgregarProductoAlCarrito(ProductoDto producto)
        {
            // Verificar si ya existe en el carrito
            var itemExistente = Items.FirstOrDefault(i => i.ProductoId == producto.Id);

            if (itemExistente != null)
            {
                // Sumar cantidad
                itemExistente.Cantidad += 1;
                itemExistente.CalcularTotales();
                ItemSeleccionado = itemExistente;
            }
            else
            {
                // Agregar nuevo item
                var nuevoItem = new ItemCarrito
                {
                    ProductoId = producto.Id,
                    Codigo = producto.Codigo,
                    Nombre = producto.Nombre,
                    PrecioUnitario = producto.PrecioVenta,
                    Cantidad = 1,
                    PorcentajeIVA = (decimal)producto.PorcentajeIVA,
                    Descuento = 0,
                    CostoUnitario = producto.PrecioCompra
                };

                nuevoItem.CalcularTotales();
                Items.Add(nuevoItem);
                ItemSeleccionado = nuevoItem;
            }

            CalcularTotales();
            CodigoBusqueda = string.Empty;
            MostrarResultados = false;
            ProductoNoEncontrado = false;

            System.Media.SystemSounds.Asterisk.Play();
        }

        [RelayCommand]
        private void AumentarCantidad()
        {
            if (ItemSeleccionado == null)
            {
                if (Items.Any()) ItemSeleccionado = Items.Last();
                else return;
            }

            ItemSeleccionado.Cantidad += 1;
            ItemSeleccionado.CalcularTotales();
            CalcularTotales();
        }

        [RelayCommand]
        private void DisminuirCantidad()
        {
            if (ItemSeleccionado == null)
            {
                if (Items.Any()) ItemSeleccionado = Items.Last();
                else return;
            }

            if (ItemSeleccionado.Cantidad > 1)
            {
                ItemSeleccionado.Cantidad -= 1;
                ItemSeleccionado.CalcularTotales();
                CalcularTotales();
            }
            else
            {
                // Si llega a 0, preguntar si eliminar
                QuitarItem();
            }
        }

        [RelayCommand]
        private void QuitarItem()
        {
            if (ItemSeleccionado == null)
            {
                MensajeError = "Seleccione un producto para quitar";
                return;
            }

            Items.Remove(ItemSeleccionado);
            ItemSeleccionado = Items.LastOrDefault();
            CalcularTotales();
        }

        [RelayCommand]
        private void SeleccionarItemAnterior()
        {
            if (!Items.Any()) return;

            if (ItemSeleccionado == null)
            {
                ItemSeleccionado = Items.Last();
                return;
            }

            var index = Items.IndexOf(ItemSeleccionado);
            if (index > 0)
                ItemSeleccionado = Items[index - 1];
        }

        [RelayCommand]
        private void SeleccionarItemSiguiente()
        {
            if (!Items.Any()) return;

            if (ItemSeleccionado == null)
            {
                ItemSeleccionado = Items.First();
                return;
            }

            var index = Items.IndexOf(ItemSeleccionado);
            if (index < Items.Count - 1)
                ItemSeleccionado = Items[index + 1];
        }

        [RelayCommand]
        private void CancelarVenta()
        {
            if (Items.Any())
            {
                var result = MessageBox.Show(
                    "¿Está seguro de cancelar esta venta?",
                    "Confirmar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            _onCerrarPestana?.Invoke(this);
        }

        [RelayCommand]
        private void ProcesarPago()
        {
            if (!Items.Any())
            {
                MessageBox.Show(
                    "Agregue productos para procesar el pago",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var ventaService = App.Services.GetRequiredService<IVentaService>();

            var viewModel = new ProcesarPagoViewModel(
                ventaService,
                Items.ToList(),
                Total,
                _cajaId,
                ClienteSeleccionado?.Id,  // ✅ AGREGAR - pasar el cliente
                () => LimpiarVenta());

            var window = new ProcesarPagoView(viewModel);
            window.ShowDialog();
        }

        private void CalcularTotales()
        {
            Subtotal = Items.Sum(i => i.Subtotal);
            IvaTotal = Items.Sum(i => i.MontoIVA);
            Total = Items.Sum(i => i.Total);
            CarritoVacio = !Items.Any();

            ActualizarTitulo();
        }

        private void ActualizarTitulo()
        {
            Titulo = Items.Any()
                ? $"Venta #{NumeroPestana} - {Total:C}"
                : $"Venta #{NumeroPestana}";
        }

        public void LimpiarVenta()
        {
            Items.Clear();
            CodigoBusqueda = string.Empty;
            MensajeError = string.Empty;
            ProductoNoEncontrado = false;
            MostrarResultados = false;
            ResultadosBusqueda.Clear();
            CarritoVacio = true;
            
            // ✅ LIMPIAR CLIENTE TAMBIÉN
            ClienteSeleccionado = null;
            BusquedaCliente = string.Empty;
            ClientesEncontrados.Clear();
            MostrarResultadosClientes = false;
            
            CalcularTotales();
        }
        public void AgregarProductoPorPeso(ProductoDto producto, decimal peso)
        {
            try
            {
                // Verificar si el producto ya está en el carrito
                var itemExistente = Items.FirstOrDefault(i => i.ProductoId == producto.Id);

                if (itemExistente != null)
                {
                    // Sumar peso
                    itemExistente.Cantidad += peso;
                    itemExistente.CalcularTotales();
                }
                else
                {
                    // Agregar nuevo item
                    var nuevoItem = new ItemCarrito
                    {
                        ProductoId = producto.Id,
                        Codigo = producto.Codigo,
                        Nombre = producto.Nombre,
                        Cantidad = peso,
                        PrecioUnitario = producto.PrecioPorKilo,
                        PorcentajeIVA = (decimal)producto.PorcentajeIVA,
                        Descuento = 0,
                        CostoUnitario = producto.PrecioCompra
                    };

                    nuevoItem.CalcularTotales();
                    Items.Add(nuevoItem);
                }

                // Actualizar totales
                CalcularTotales();
                CarritoVacio = !Items.Any();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}