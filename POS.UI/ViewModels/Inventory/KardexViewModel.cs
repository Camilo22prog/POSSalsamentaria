using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Inventory;
using POS.Application.Interfaces;
using System.Collections.ObjectModel;

namespace POS.UI.ViewModels.Inventory
{
    public partial class KardexViewModel : ObservableObject
    {
        private readonly IInventarioService _inventarioService;
        private readonly int _productoId;

        [ObservableProperty]
        private ObservableCollection<MovimientoInventarioDto> _movimientos = new();

        [ObservableProperty]
        private DateTime? _fechaDesde;

        [ObservableProperty]
        private DateTime? _fechaHasta;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        public KardexViewModel(IInventarioService inventarioService, int productoId)
        {
            _inventarioService = inventarioService;
            _productoId = productoId;

            // Por defecto, últimos 30 días
            FechaDesde = DateTime.Now.AddDays(-30);
            FechaHasta = DateTime.Now;
        }

        public async Task CargarAsync()
        {
            await CargarMovimientosAsync();
        }

        [RelayCommand]
        private async Task CargarMovimientosAsync()
        {
            IsLoading = true;
            MensajeError = string.Empty;

            try
            {
                var movimientos = await _inventarioService.ObtenerKardexPorProductoAsync(
                    _productoId,
                    FechaDesde,
                    FechaHasta);

                Movimientos.Clear();
                foreach (var movimiento in movimientos)
                {
                    Movimientos.Add(movimiento);
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar kardex: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}