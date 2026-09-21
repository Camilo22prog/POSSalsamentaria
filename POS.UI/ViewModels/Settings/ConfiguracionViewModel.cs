using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Settings;
using POS.Application.Interfaces;
using POS.UI.Services;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO.Ports;
using System.Windows;

namespace POS.UI.ViewModels.Settings
{
    public partial class ConfiguracionViewModel : ObservableObject
    {
        private readonly IConfiguracionService _configuracionService;
        private readonly IUpdateService _updateService;

        // ── Datos del negocio ────────────────────────────────────────────────
        [ObservableProperty] private string _nombreNegocio = string.Empty;
        [ObservableProperty] private string _nit = string.Empty;
        [ObservableProperty] private string _direccion = string.Empty;
        [ObservableProperty] private string _telefono = string.Empty;
        [ObservableProperty] private string _mensajePieTicket = string.Empty;

        // ── Impresora ────────────────────────────────────────────────────────
        [ObservableProperty] private string? _impresoraSeleccionada;
        [ObservableProperty] private bool _abrirCajonAutomatico = true;
        [ObservableProperty] private ObservableCollection<string> _impresorasDisponibles = new();

        // ── Balanza ──────────────────────────────────────────────────────────
        [ObservableProperty] private string? _puertoSeleccionado;
        [ObservableProperty] private int _baudRateSeleccionado = 9600;
        [ObservableProperty] private ObservableCollection<string> _puertosDisponibles = new();

        // ── Actualizaciones ──────────────────────────────────────────────────
        [ObservableProperty] private string _urlActualizaciones = string.Empty;

        // ── Estado ───────────────────────────────────────────────────────────
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _mensajeEstado = string.Empty;
        [ObservableProperty] private bool _guardadoExitoso;

        public IEnumerable<int> BaudRatesDisponibles { get; } =
            new[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };

        public ConfiguracionViewModel(IConfiguracionService configuracionService, IUpdateService updateService)
        {
            _configuracionService = configuracionService;
            _updateService = updateService;
            CargarDispositivos();
            _ = CargarAsync();
        }

        private void CargarDispositivos()
        {
            // Impresoras instaladas + opción auto-detect
            ImpresorasDisponibles.Clear();
            ImpresorasDisponibles.Add("(Auto-detectar)");
            foreach (string p in PrinterSettings.InstalledPrinters)
                ImpresorasDisponibles.Add(p);

            // Puertos COM disponibles + opción auto-detect
            PuertosDisponibles.Clear();
            PuertosDisponibles.Add("(Auto-detectar)");
            foreach (string p in SerialPort.GetPortNames().OrderBy(x => x))
                PuertosDisponibles.Add(p);
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            IsLoading = true;
            try
            {
                var dto = await _configuracionService.ObtenerAsync();
                NombreNegocio        = dto.NombreNegocio;
                Nit                  = dto.Nit ?? string.Empty;
                Direccion            = dto.Direccion ?? string.Empty;
                Telefono             = dto.Telefono ?? string.Empty;
                MensajePieTicket     = dto.MensajePieTicket;
                AbrirCajonAutomatico = dto.AbrirCajonAutomatico;
                BaudRateSeleccionado = dto.BaudRateBalanza;
                UrlActualizaciones   = dto.UrlActualizaciones ?? string.Empty;

                ImpresoraSeleccionada = string.IsNullOrEmpty(dto.NombreImpresora)
                    ? "(Auto-detectar)"
                    : dto.NombreImpresora;

                PuertoSeleccionado = string.IsNullOrEmpty(dto.PuertoBalanza)
                    ? "(Auto-detectar)"
                    : dto.PuertoBalanza;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar configuración: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreNegocio))
            {
                MessageBox.Show("El nombre del negocio es requerido.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            MensajeEstado = string.Empty;
            GuardadoExitoso = false;
            try
            {
                var dto = new ConfiguracionDto
                {
                    NombreNegocio       = NombreNegocio.Trim(),
                    Nit                 = string.IsNullOrWhiteSpace(Nit) ? null : Nit.Trim(),
                    Direccion           = string.IsNullOrWhiteSpace(Direccion) ? null : Direccion.Trim(),
                    Telefono            = string.IsNullOrWhiteSpace(Telefono) ? null : Telefono.Trim(),
                    MensajePieTicket    = MensajePieTicket.Trim(),
                    NombreImpresora     = ImpresoraSeleccionada == "(Auto-detectar)" ? null : ImpresoraSeleccionada,
                    AbrirCajonAutomatico = AbrirCajonAutomatico,
                    PuertoBalanza       = PuertoSeleccionado == "(Auto-detectar)" ? null : PuertoSeleccionado,
                    BaudRateBalanza     = BaudRateSeleccionado,
                    UrlActualizaciones  = string.IsNullOrWhiteSpace(UrlActualizaciones) ? null : UrlActualizaciones.Trim(),
                };

                await _configuracionService.GuardarAsync(dto);

                // Actualizar el singleton en memoria
                AppConfig.Cargar(dto);
                _updateService.ConfigurarUrl(dto.UrlActualizaciones);

                MensajeEstado = "✅ Configuración guardada correctamente.";
                GuardadoExitoso = true;
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error: {ex.Message}";
                GuardadoExitoso = false;
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void RefrescarDispositivos()
        {
            var impActual = ImpresoraSeleccionada;
            var puertoActual = PuertoSeleccionado;
            CargarDispositivos();
            ImpresoraSeleccionada = ImpresorasDisponibles.Contains(impActual ?? "") ? impActual : "(Auto-detectar)";
            PuertoSeleccionado = PuertosDisponibles.Contains(puertoActual ?? "") ? puertoActual : "(Auto-detectar)";
        }
    }
}
