using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Customers;
using POS.Application.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace POS.UI.ViewModels.Customers
{
    public partial class ClientesViewModel : ObservableObject
    {
        private readonly IClienteService _clienteService;

        [ObservableProperty]
        private ObservableCollection<ClienteDto> _clientes = new();

        [ObservableProperty]
        private ClienteDto? _clienteSeleccionado;

        [ObservableProperty]
        private string _terminoBusqueda = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public ClientesViewModel(IClienteService clienteService)
        {
            _clienteService = clienteService;
            _ = CargarClientesAsync();
        }

        [RelayCommand]
        private async Task CargarClientesAsync()
        {
            IsLoading = true;

            try
            {
                var clientes = await _clienteService.ObtenerTodosAsync();
                Clientes = new ObservableCollection<ClienteDto>(clientes.Where(c => c.Activo));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task BuscarAsync()
        {
            if (string.IsNullOrWhiteSpace(TerminoBusqueda))
            {
                await CargarClientesAsync();
                return;
            }

            IsLoading = true;

            try
            {
                var clientes = await _clienteService.BuscarAsync(TerminoBusqueda);
                Clientes = new ObservableCollection<ClienteDto>(clientes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NuevoCliente()
        {
            var formViewModel = App.Services.GetRequiredService<ClienteFormViewModel>();
            var formView = new POS.UI.Views.Customers.ClienteFormView(formViewModel);
            
            if (formView.ShowDialog() == true)
            {
                _ = CargarClientesAsync();
            }
        }

        [RelayCommand]
        private void EditarCliente()
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Seleccione un cliente", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var formViewModel = App.Services.GetRequiredService<ClienteFormViewModel>();
            formViewModel.CargarCliente(ClienteSeleccionado.Id);
            
            var formView = new POS.UI.Views.Customers.ClienteFormView(formViewModel);
            
            if (formView.ShowDialog() == true)
            {
                _ = CargarClientesAsync();
            }
        }

        [RelayCommand]
        private async Task EliminarClienteAsync()
        {
            if (ClienteSeleccionado == null)
            {
                MessageBox.Show("Seleccione un cliente", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"¿Está seguro de eliminar al cliente {ClienteSeleccionado.NombreCompleto}?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;

            try
            {
                await _clienteService.EliminarAsync(ClienteSeleccionado.Id);
                await CargarClientesAsync();
                
                MessageBox.Show("Cliente eliminado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}