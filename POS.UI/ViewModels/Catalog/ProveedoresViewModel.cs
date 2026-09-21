using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Catalog;
using POS.Application.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Catalog
{
    public partial class ProveedoresViewModel : ObservableObject
    {
        private readonly IProveedorService _proveedorService;

        [ObservableProperty]
        private ObservableCollection<ProveedorDto> _proveedores = new();

        [ObservableProperty]
        private ProveedorDto? _proveedorSeleccionado;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _mostrandoFormulario;

        [ObservableProperty]
        private bool _editando;

        [ObservableProperty]
        private string _nombreForm = string.Empty;

        [ObservableProperty]
        private string _nombreComercialForm = string.Empty;

        [ObservableProperty]
        private string _telefonoForm = string.Empty;

        [ObservableProperty]
        private string _emailForm = string.Empty;

        [ObservableProperty]
        private string _direccionForm = string.Empty;

        [ObservableProperty]
        private string _personaContactoForm = string.Empty;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        private int? _editandoId;

        public ProveedoresViewModel(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
            _ = CargarAsync();
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            IsLoading = true;
            try
            {
                var lista = await _proveedorService.ObtenerTodosAsync();
                Proveedores = new ObservableCollection<ProveedorDto>(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar proveedores: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void NuevoProveedor()
        {
            _editandoId = null;
            Editando = false;
            LimpiarFormulario();
            MostrandoFormulario = true;
        }

        [RelayCommand]
        private void EditarProveedor()
        {
            if (ProveedorSeleccionado == null)
            {
                MessageBox.Show("Seleccione un proveedor para editar.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _editandoId = ProveedorSeleccionado.Id;
            Editando = true;
            NombreForm = ProveedorSeleccionado.Nombre;
            NombreComercialForm = ProveedorSeleccionado.NombreComercial ?? string.Empty;
            TelefonoForm = ProveedorSeleccionado.Telefono ?? string.Empty;
            EmailForm = ProveedorSeleccionado.Email ?? string.Empty;
            DireccionForm = ProveedorSeleccionado.Direccion ?? string.Empty;
            PersonaContactoForm = ProveedorSeleccionado.PersonaContacto ?? string.Empty;
            MensajeError = string.Empty;
            MostrandoFormulario = true;
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreForm))
            {
                MensajeError = "El nombre es requerido.";
                return;
            }

            IsLoading = true;
            MensajeError = string.Empty;
            try
            {
                var dto = new CrearProveedorDto
                {
                    Nombre = NombreForm.Trim(),
                    NombreComercial = string.IsNullOrWhiteSpace(NombreComercialForm) ? null : NombreComercialForm.Trim(),
                    Telefono = string.IsNullOrWhiteSpace(TelefonoForm) ? null : TelefonoForm.Trim(),
                    Email = string.IsNullOrWhiteSpace(EmailForm) ? null : EmailForm.Trim(),
                    Direccion = string.IsNullOrWhiteSpace(DireccionForm) ? null : DireccionForm.Trim(),
                    PersonaContacto = string.IsNullOrWhiteSpace(PersonaContactoForm) ? null : PersonaContactoForm.Trim()
                };

                if (_editandoId.HasValue)
                    await _proveedorService.ActualizarAsync(_editandoId.Value, dto);
                else
                    await _proveedorService.CrearAsync(dto);

                MostrandoFormulario = false;
                await CargarAsync();
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void CancelarFormulario()
        {
            MostrandoFormulario = false;
            MensajeError = string.Empty;
        }

        [RelayCommand]
        private async Task DesactivarAsync()
        {
            if (ProveedorSeleccionado == null)
            {
                MessageBox.Show("Seleccione un proveedor.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var r = MessageBox.Show($"¿Desactivar '{ProveedorSeleccionado.Nombre}'?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;

            try
            {
                await _proveedorService.DesactivarAsync(ProveedorSeleccionado.Id);
                await CargarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ActivarAsync()
        {
            if (ProveedorSeleccionado == null)
            {
                MessageBox.Show("Seleccione un proveedor.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                await _proveedorService.ActivarAsync(ProveedorSeleccionado.Id);
                await CargarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarFormulario()
        {
            NombreForm = string.Empty;
            NombreComercialForm = string.Empty;
            TelefonoForm = string.Empty;
            EmailForm = string.Empty;
            DireccionForm = string.Empty;
            PersonaContactoForm = string.Empty;
            MensajeError = string.Empty;
        }
    }
}
