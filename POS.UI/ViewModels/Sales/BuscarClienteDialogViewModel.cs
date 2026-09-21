using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Customers;
using POS.Application.Interfaces;
using POS.UI.ViewModels.Customers;
using POS.UI.Views.Customers;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Sales
{
    public partial class BuscarClienteDialogViewModel : ObservableObject
    {
        private readonly IClienteService _clienteService;

        // Cliente ya seleccionado en la pestaña al abrir el diálogo
        public ClienteDto? ClienteInicial { get; }

        // Resultado: cliente elegido de la lista
        public ClienteDto? ClienteElegido { get; private set; }

        // Resultado: el usuario pidió quitar el cliente
        public bool Limpiado { get; private set; }

        public event Action? SolicitarCierre;

        [ObservableProperty]
        private string _busqueda = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ClienteDto> _resultados = new();

        [ObservableProperty]
        private bool _isLoading;

        public BuscarClienteDialogViewModel(IClienteService clienteService, ClienteDto? clienteActual)
        {
            _clienteService = clienteService;
            ClienteInicial = clienteActual;
        }

        partial void OnBusquedaChanged(string value)
        {
            if (value.Length >= 2)
                _ = BuscarAsync(value);
            else
                Resultados.Clear();
        }

        private async Task BuscarAsync(string termino)
        {
            IsLoading = true;
            try
            {
                var clientes = await _clienteService.BuscarAsync(termino);
                Resultados = new ObservableCollection<ClienteDto>(clientes.Take(10));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar clientes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void Seleccionar(ClienteDto cliente)
        {
            ClienteElegido = cliente;
            Limpiado = false;
            SolicitarCierre?.Invoke();
        }

        [RelayCommand]
        private void Limpiar()
        {
            ClienteElegido = null;
            Limpiado = true;
            SolicitarCierre?.Invoke();
        }

        [RelayCommand]
        private void AbrirNuevoCliente()
        {
            try
            {
                var vm = new ClienteFormViewModel(_clienteService);
                var view = new ClienteFormView(vm);
                if (view.ShowDialog() == true && !string.IsNullOrWhiteSpace(Busqueda))
                    _ = BuscarAsync(Busqueda);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir formulario: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
