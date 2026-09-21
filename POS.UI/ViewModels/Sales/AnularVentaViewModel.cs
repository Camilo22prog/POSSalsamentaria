using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.Interfaces;
using POS.UI.Services;
using System.Windows;
using System.Linq;

namespace POS.UI.ViewModels.Sales
{
    public partial class AnularVentaViewModel : ObservableObject
    {
        private readonly IVentaService _ventaService;
        private readonly int _ventaId;
        private readonly string _numeroVenta;
        private readonly Action _onVentaAnulada;

        [ObservableProperty]
        private string _titulo;

        [ObservableProperty]
        private string _motivo = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public AnularVentaViewModel(
            IVentaService ventaService,
            int ventaId,
            string numeroVenta,
            Action onVentaAnulada)
        {
            _ventaService = ventaService;
            _ventaId = ventaId;
            _numeroVenta = numeroVenta;
            _onVentaAnulada = onVentaAnulada;

            Titulo = $"Anular Venta {numeroVenta}";
        }

        [RelayCommand]
        private async Task AnularAsync()
        {
            if (string.IsNullOrWhiteSpace(Motivo))
            {
                MessageBox.Show(
                    "Debe especificar el motivo de la anulación",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"⚠️ ¿Está seguro que desea ANULAR la venta {_numeroVenta}?\n\n" +
                $"Esta acción:\n" +
                $"• Marcará la venta como anulada\n" +
                $"• Devolverá el inventario\n" +
                $"• NO se puede deshacer\n\n" +
                $"Motivo: {Motivo}",
                "Confirmar Anulación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;

            try
            {
                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                await _ventaService.AnularVentaAsync(_ventaId, Motivo, usuarioId);

                MessageBox.Show(
                    $"Venta {_numeroVenta} anulada exitosamente.\n\n" +
                    $"El inventario ha sido revertido.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _onVentaAnulada?.Invoke();

                // Cerrar ventana
                System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)?
                    .Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al anular venta:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
                .FirstOrDefault(w => w.DataContext == this)?
                .Close();
        }
    }
}