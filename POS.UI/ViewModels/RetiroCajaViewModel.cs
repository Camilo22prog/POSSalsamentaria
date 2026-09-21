using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Sales;
using POS.Application.Interfaces;
using POS.UI.Services;
using System.Windows;

namespace POS.UI.ViewModels.Sales
{
    public partial class RetiroCajaViewModel : ObservableObject
    {
        private readonly ICajaService _cajaService;
        private readonly IAuthService _authService;
        private readonly int _cajaId;

        // ── Datos del retiro ─────────────────────────────────────────────────

        [ObservableProperty]
        private decimal _monto;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EsOtros))]
        [NotifyPropertyChangedFor(nameof(LabelObservaciones))]
        private string _motivoSeleccionado = string.Empty;

        [ObservableProperty]
        private string _descripcionOtros = string.Empty;

        [ObservableProperty]
        private string _observaciones = string.Empty;

        // ── Autorización ─────────────────────────────────────────────────────

        [ObservableProperty]
        private string _usuarioAutorizador = string.Empty;

        // La contraseña la gestiona el code-behind del PasswordBox
        public string PasswordAutorizador { get; set; } = string.Empty;

        [ObservableProperty]
        private string _supervisorAutorizado = string.Empty;

        [ObservableProperty]
        private bool _autorizado;

        // ── Estado ───────────────────────────────────────────────────────────

        [ObservableProperty]
        private bool _isLoading;

        // ── Computed ─────────────────────────────────────────────────────────

        public bool EsOtros => MotivoSeleccionado?.Trim().Equals("Otros", StringComparison.OrdinalIgnoreCase) == true;

        public string LabelObservaciones => EsOtros ? "Descripción del motivo (obligatorio): *" : "Observaciones (opcional):";

        public static IEnumerable<string> Motivos => new[]
        {
            "Gastos operativos",
            "Pago a proveedores",
            "Servicios públicos",
            "Consignación bancaria",
            "Cambio de billetes",
            "Otros"
        };

        public RetiroCajaViewModel(ICajaService cajaService, IAuthService authService, int cajaId)
        {
            _cajaService = cajaService;
            _authService = authService;
            _cajaId = cajaId;
        }

        // ── Autorización de supervisor ────────────────────────────────────────

        [RelayCommand]
        private async Task ValidarSupervisorAsync()
        {
            if (string.IsNullOrWhiteSpace(UsuarioAutorizador) || string.IsNullOrWhiteSpace(PasswordAutorizador))
            {
                MessageBox.Show("Ingrese el usuario y contraseña del supervisor.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                var (success, nombre, error) = await _authService.ValidarSupervisorAsync(UsuarioAutorizador, PasswordAutorizador);

                if (success)
                {
                    Autorizado = true;
                    SupervisorAutorizado = nombre ?? UsuarioAutorizador;
                }
                else
                {
                    MessageBox.Show(error ?? "Autorización fallida.",
                        "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ── Registrar retiro ─────────────────────────────────────────────────

        [RelayCommand]
        private async Task RegistrarAsync()
        {
            if (!Autorizado)
            {
                MessageBox.Show("Debe autorizar el retiro con un supervisor o administrador.",
                    "Sin Autorización", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Monto <= 0)
            {
                MessageBox.Show("El monto debe ser mayor a cero.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(MotivoSeleccionado))
            {
                MessageBox.Show("Debe seleccionar un motivo.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EsOtros && string.IsNullOrWhiteSpace(DescripcionOtros))
            {
                MessageBox.Show("Cuando el motivo es 'Otros', debe describir el motivo del retiro.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                var motivoFinal = EsOtros
                    ? $"Otros: {DescripcionOtros.Trim()}"
                    : MotivoSeleccionado;

                var obs = string.IsNullOrWhiteSpace(Observaciones)
                    ? $"Autorizado por: {SupervisorAutorizado}"
                    : $"{Observaciones.Trim()} | Autorizado por: {SupervisorAutorizado}";

                var dto = new CrearRetiroDto
                {
                    Monto = Monto,
                    Motivo = motivoFinal,
                    Observaciones = obs
                };

                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                await _cajaService.RegistrarRetiroAsync(_cajaId, dto, usuarioId);

                MessageBox.Show(
                    $"Retiro registrado exitosamente.\n\n" +
                    $"Monto: {Monto:C0}\n" +
                    $"Motivo: {motivoFinal}\n" +
                    $"Autorizado por: {SupervisorAutorizado}",
                    "Retiro Registrado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)
                    ?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar retiro: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Cancelar()
        {
            System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }
    }
}
