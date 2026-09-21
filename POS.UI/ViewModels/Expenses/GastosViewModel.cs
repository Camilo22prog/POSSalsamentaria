using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Expenses;
using POS.Application.Interfaces;
using POS.Domain.Enums;
using POS.UI.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Expenses
{
    public partial class GastosViewModel : ObservableObject
    {
        private readonly IGastoService _gastoService;

        [ObservableProperty] private ObservableCollection<GastoOperativoDto> _gastos = new();
        [ObservableProperty] private GastoOperativoDto? _gastoSeleccionado;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _mostrandoFormulario;
        [ObservableProperty] private bool _editando;

        // Filtros
        [ObservableProperty] private DateTime _fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        [ObservableProperty] private DateTime _fechaFin = DateTime.Now;

        // Formulario
        [ObservableProperty] private TipoGasto _tipoForm = TipoGasto.Otro;
        [ObservableProperty] private string _descripcionForm = string.Empty;
        [ObservableProperty] private decimal _montoForm;
        [ObservableProperty] private DateTime _fechaForm = DateTime.Now;
        [ObservableProperty] private string _comprobanteForm = string.Empty;
        [ObservableProperty] private string _observacionesForm = string.Empty;
        [ObservableProperty] private string _mensajeError = string.Empty;

        // Totalizador
        [ObservableProperty] private decimal _totalPeriodo;

        private int? _editandoId;

        public IEnumerable<TipoGasto> TiposGasto => Enum.GetValues<TipoGasto>();

        public GastosViewModel(IGastoService gastoService)
        {
            _gastoService = gastoService;
            _ = CargarAsync();
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            IsLoading = true;
            try
            {
                var lista = await _gastoService.ObtenerPorFechaAsync(FechaInicio, FechaFin);
                Gastos = new ObservableCollection<GastoOperativoDto>(lista);
                TotalPeriodo = Gastos.Sum(g => g.Monto);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar gastos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void NuevoGasto()
        {
            _editandoId = null;
            Editando = false;
            LimpiarFormulario();
            MostrandoFormulario = true;
        }

        [RelayCommand]
        private void EditarGasto()
        {
            if (GastoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un gasto para editar.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _editandoId = GastoSeleccionado.Id;
            Editando = true;
            TipoForm         = GastoSeleccionado.Tipo;
            DescripcionForm  = GastoSeleccionado.Descripcion;
            MontoForm        = GastoSeleccionado.Monto;
            FechaForm        = GastoSeleccionado.Fecha;
            ComprobanteForm  = GastoSeleccionado.Comprobante ?? string.Empty;
            ObservacionesForm = GastoSeleccionado.Observaciones ?? string.Empty;
            MensajeError = string.Empty;
            MostrandoFormulario = true;
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (string.IsNullOrWhiteSpace(DescripcionForm))
            {
                MensajeError = "La descripción es requerida.";
                return;
            }
            if (MontoForm <= 0)
            {
                MensajeError = "El monto debe ser mayor a cero.";
                return;
            }

            IsLoading = true;
            MensajeError = string.Empty;
            try
            {
                var dto = new CrearGastoOperativoDto
                {
                    Tipo         = TipoForm,
                    Descripcion  = DescripcionForm,
                    Monto        = MontoForm,
                    Fecha        = FechaForm,
                    Comprobante  = string.IsNullOrWhiteSpace(ComprobanteForm) ? null : ComprobanteForm,
                    Observaciones = string.IsNullOrWhiteSpace(ObservacionesForm) ? null : ObservacionesForm,
                    UsuarioId    = SessionService.Instance.UsuarioActual?.Id ?? 0
                };

                if (_editandoId.HasValue)
                    await _gastoService.ActualizarAsync(_editandoId.Value, dto);
                else
                    await _gastoService.CrearAsync(dto);

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
        private async Task EliminarAsync()
        {
            if (GastoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un gasto para eliminar.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var r = MessageBox.Show(
                $"¿Eliminar el gasto '{GastoSeleccionado.Descripcion}' por {GastoSeleccionado.Monto:C}?",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (r != MessageBoxResult.Yes) return;

            try
            {
                await _gastoService.EliminarAsync(GastoSeleccionado.Id);
                await CargarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void EstablecerRangoRapido(string rango)
        {
            var hoy = DateTime.Now;
            switch (rango)
            {
                case "Mes":
                    FechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    FechaFin = hoy;
                    break;
                case "MesAnterior":
                    var mesAnt = hoy.AddMonths(-1);
                    FechaInicio = new DateTime(mesAnt.Year, mesAnt.Month, 1);
                    FechaFin = new DateTime(mesAnt.Year, mesAnt.Month,
                        DateTime.DaysInMonth(mesAnt.Year, mesAnt.Month));
                    break;
                case "Trimestre":
                    FechaInicio = hoy.AddMonths(-3);
                    FechaFin = hoy;
                    break;
                case "Año":
                    FechaInicio = new DateTime(hoy.Year, 1, 1);
                    FechaFin = hoy;
                    break;
            }
        }

        private void LimpiarFormulario()
        {
            TipoForm         = TipoGasto.Otro;
            DescripcionForm  = string.Empty;
            MontoForm        = 0;
            FechaForm        = DateTime.Now;
            ComprobanteForm  = string.Empty;
            ObservacionesForm = string.Empty;
            MensajeError     = string.Empty;
        }
    }
}
