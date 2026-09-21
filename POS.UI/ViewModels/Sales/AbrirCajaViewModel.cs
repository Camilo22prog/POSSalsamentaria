using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Application.DTOs.Sales;
using POS.Application.Interfaces;
using POS.UI.Models;
using POS.UI.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.UI.ViewModels.Sales
{
    public partial class AbrirCajaViewModel : ObservableObject
    {
        private readonly ICajaService _cajaService;

        [ObservableProperty]
        private ObservableCollection<DenominacionItem> _monedas = new();

        [ObservableProperty]
        private ObservableCollection<DenominacionItem> _billetes = new();

        [ObservableProperty]
        private decimal _totalMonedas;

        [ObservableProperty]
        private decimal _totalBilletes;

        [ObservableProperty]
        private decimal _totalGeneral;

        [ObservableProperty]
        private bool _isLoading;

        public AbrirCajaViewModel(ICajaService cajaService)
        {
            _cajaService = cajaService;
            InicializarDenominaciones();
        }

        private void InicializarDenominaciones()
        {
            // Monedas
            Monedas.Add(new DenominacionItem { Nombre = "$50", Valor = 50, Cantidad = 0 });
            Monedas.Add(new DenominacionItem { Nombre = "$100", Valor = 100, Cantidad = 0 });
            Monedas.Add(new DenominacionItem { Nombre = "$200", Valor = 200, Cantidad = 0 });
            Monedas.Add(new DenominacionItem { Nombre = "$500", Valor = 500, Cantidad = 0 });
            Monedas.Add(new DenominacionItem { Nombre = "$1,000", Valor = 1000, Cantidad = 0 });

            // Billetes
            Billetes.Add(new DenominacionItem { Nombre = "$2,000", Valor = 2000, Cantidad = 0 });
            Billetes.Add(new DenominacionItem { Nombre = "$5,000", Valor = 5000, Cantidad = 0 });
            Billetes.Add(new DenominacionItem { Nombre = "$10,000", Valor = 10000, Cantidad = 0 });
            Billetes.Add(new DenominacionItem { Nombre = "$20,000", Valor = 20000, Cantidad = 0 });
            Billetes.Add(new DenominacionItem { Nombre = "$50,000", Valor = 50000, Cantidad = 0 });
            Billetes.Add(new DenominacionItem { Nombre = "$100,000", Valor = 100000, Cantidad = 0 });

            // Suscribirse a cambios
            foreach (var moneda in Monedas)
            {
                moneda.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(DenominacionItem.Total))
                        CalcularTotales();
                };
            }

            foreach (var billete in Billetes)
            {
                billete.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(DenominacionItem.Total))
                        CalcularTotales();
                };
            }
        }

        private void CalcularTotales()
        {
            TotalMonedas = Monedas.Sum(m => m.Total);
            TotalBilletes = Billetes.Sum(b => b.Total);
            TotalGeneral = TotalMonedas + TotalBilletes;
        }

        [RelayCommand]
        private async Task AbrirCajaAsync()
        {
            if (TotalGeneral <= 0)
            {
                MessageBox.Show(
                    "Debe ingresar al menos una denominación para abrir la caja.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                var dto = new AbrirCajaConDenominacionesDto
                {
                    MontoInicial = TotalGeneral,
                    Monedas = Monedas
                        .Where(m => m.Cantidad > 0)
                        .Select(m => new DetalleDenominacionDto
                        {
                            Valor = m.Valor,
                            Cantidad = m.Cantidad,
                            Total = m.Total
                        }).ToList(),
                    Billetes = Billetes
                        .Where(b => b.Cantidad > 0)
                        .Select(b => new DetalleDenominacionDto
                        {
                            Valor = b.Valor,
                            Cantidad = b.Cantidad,
                            Total = b.Total
                        }).ToList()
                };

                var usuarioId = SessionService.Instance.UsuarioActual?.Id ?? 0;
                await _cajaService.AbrirCajaConDenominacionesAsync(dto, usuarioId);

                MessageBox.Show(
                    $"✅ Caja abierta exitosamente\n\nMonto inicial: {TotalGeneral:C}",
                    "Apertura Exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Cerrar ventana
                System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)
                    ?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir caja: {ex.Message}",
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
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }
    }
}