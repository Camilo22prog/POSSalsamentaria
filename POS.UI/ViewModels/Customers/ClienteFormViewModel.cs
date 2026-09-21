using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Customers;
using POS.Application.Interfaces;
using POS.Domain.Enums;
using System.Windows;

namespace POS.UI.ViewModels.Customers
{
    public partial class ClienteFormViewModel : ObservableObject
    {
        private readonly IClienteService _clienteService;
        private int? _clienteId;

        [ObservableProperty]
        private string _titulo = "Nuevo Cliente";

        [ObservableProperty]
        private TipoCliente _tipoCliente = TipoCliente.PersonaNatural;

        [ObservableProperty]
        private TipoDocumento _tipoDocumento = TipoDocumento.CC;

        [ObservableProperty]
        private string _numeroDocumento = string.Empty;

        [ObservableProperty]
        private string _nombreCompleto = string.Empty;

        [ObservableProperty]
        private string _razonSocial = string.Empty;

        [ObservableProperty]
        private string _nombreComercial = string.Empty;

        [ObservableProperty]
        private string _departamento = string.Empty;

        [ObservableProperty]
        private string _ciudad = string.Empty;

        [ObservableProperty]
        private string _direccion = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _telefono = string.Empty;

        [ObservableProperty]
        private string _emailSecundario = string.Empty;

        [ObservableProperty]
        private string _telefonoSecundario = string.Empty;

        [ObservableProperty]
        private ResponsabilidadFiscal _responsabilidadFiscal = ResponsabilidadFiscal.NoResponsable;

        [ObservableProperty]
        private RegimenTributario _regimen = RegimenTributario.Simplificado;

        [ObservableProperty]
        private string _actividadEconomica = string.Empty;

        [ObservableProperty]
        private string _observaciones = string.Empty;

        [ObservableProperty]
        private bool _activo = true;

        [ObservableProperty]
        private bool _isLoading;

        public ClienteFormViewModel(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        public async void CargarCliente(int clienteId)
        {
            _clienteId = clienteId;
            Titulo = "Editar Cliente";

            IsLoading = true;

            try
            {
                var cliente = await _clienteService.ObtenerPorIdAsync(clienteId);
                if (cliente == null)
                {
                    MessageBox.Show("Cliente no encontrado", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                TipoCliente = cliente.TipoCliente;
                TipoDocumento = cliente.TipoDocumento;
                NumeroDocumento = cliente.NumeroDocumento;
                NombreCompleto = cliente.NombreCompleto;
                RazonSocial = cliente.RazonSocial ?? string.Empty;
                NombreComercial = cliente.NombreComercial ?? string.Empty;
                Departamento = cliente.Departamento;
                Ciudad = cliente.Ciudad;
                Direccion = cliente.Direccion;
                Email = cliente.Email;
                Telefono = cliente.Telefono;
                EmailSecundario = cliente.EmailSecundario ?? string.Empty;
                TelefonoSecundario = cliente.TelefonoSecundario ?? string.Empty;
                ResponsabilidadFiscal = cliente.ResponsabilidadFiscal;
                Regimen = cliente.Regimen;
                ActividadEconomica = cliente.ActividadEconomica ?? string.Empty;
                Observaciones = cliente.Observaciones ?? string.Empty;
                Activo = cliente.Activo;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (!Validar())
                return;

            IsLoading = true;

            try
            {
                if (_clienteId.HasValue)
                {
                    // Actualizar
                    var dto = new ActualizarClienteDto
                    {
                        Id = _clienteId.Value,
                        TipoCliente = TipoCliente,
                        TipoDocumento = TipoDocumento,
                        NumeroDocumento = NumeroDocumento,
                        NombreCompleto = NombreCompleto,
                        RazonSocial = string.IsNullOrWhiteSpace(RazonSocial) ? null : RazonSocial,
                        NombreComercial = string.IsNullOrWhiteSpace(NombreComercial) ? null : NombreComercial,
                        Departamento = Departamento,
                        Ciudad = Ciudad,
                        Direccion = Direccion,
                        Email = Email,
                        Telefono = Telefono,
                        EmailSecundario = string.IsNullOrWhiteSpace(EmailSecundario) ? null : EmailSecundario,
                        TelefonoSecundario = string.IsNullOrWhiteSpace(TelefonoSecundario) ? null : TelefonoSecundario,
                        ResponsabilidadFiscal = ResponsabilidadFiscal,
                        Regimen = Regimen,
                        ActividadEconomica = string.IsNullOrWhiteSpace(ActividadEconomica) ? null : ActividadEconomica,
                        Observaciones = string.IsNullOrWhiteSpace(Observaciones) ? null : Observaciones,
                        Activo = Activo
                    };

                    await _clienteService.ActualizarAsync(dto);
                }
                else
                {
                    // Crear
                    var dto = new CrearClienteDto
                    {
                        TipoCliente = TipoCliente,
                        TipoDocumento = TipoDocumento,
                        NumeroDocumento = NumeroDocumento,
                        NombreCompleto = NombreCompleto,
                        RazonSocial = string.IsNullOrWhiteSpace(RazonSocial) ? null : RazonSocial,
                        NombreComercial = string.IsNullOrWhiteSpace(NombreComercial) ? null : NombreComercial,
                        Departamento = Departamento,
                        Ciudad = Ciudad,
                        Direccion = Direccion,
                        Email = Email,
                        Telefono = Telefono,
                        EmailSecundario = string.IsNullOrWhiteSpace(EmailSecundario) ? null : EmailSecundario,
                        TelefonoSecundario = string.IsNullOrWhiteSpace(TelefonoSecundario) ? null : TelefonoSecundario,
                        ResponsabilidadFiscal = ResponsabilidadFiscal,
                        Regimen = Regimen,
                        ActividadEconomica = string.IsNullOrWhiteSpace(ActividadEconomica) ? null : ActividadEconomica,
                        Observaciones = string.IsNullOrWhiteSpace(Observaciones) ? null : Observaciones
                    };

                    await _clienteService.CrearAsync(dto);
                }

                // Cerrar ventana con éxito
                System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)?
                    .Close();

                MessageBox.Show("Cliente guardado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(NumeroDocumento))
            {
                MessageBox.Show("El número de documento es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(NombreCompleto))
            {
                MessageBox.Show("El nombre completo es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("El email es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Telefono))
            {
                MessageBox.Show("El teléfono es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Departamento))
            {
                MessageBox.Show("El departamento es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Ciudad))
            {
                MessageBox.Show("La ciudad es obligatoria", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Direccion))
            {
                MessageBox.Show("La dirección es obligatoria", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        [RelayCommand]
        private void Cancelar()
        {
            System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?
                .Close();
        }
    }
}