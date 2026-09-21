using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using POS.UI.Views.Reports;
using System;
using System.Windows;
using System.Windows.Controls;

namespace POS.UI.ViewModels.Reports
{
    public partial class ReportesMenuViewModel : ObservableObject
    {
        [ObservableProperty]
        private UserControl? _contenidoActual;

        public ReportesMenuViewModel()
        {
            System.Diagnostics.Debug.WriteLine("✅ ReportesMenuViewModel creado");
        }

        [RelayCommand]
        private void AbrirReporte(string tipo)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 AbrirReporte llamado con tipo: {tipo}");
            
            try
            {
                UserControl? vista = tipo switch
                {
                    "VentasPeriodo" => App.Services.GetRequiredService<VentasPorPeriodoView>(),
                    "VentasProducto" => App.Services.GetRequiredService<VentasPorProductoView>(),
                    _ => null
                };
                
                System.Diagnostics.Debug.WriteLine($"📊 Vista creada: {vista?.GetType().Name ?? "NULL"}");
                
                ContenidoActual = vista;
                
                System.Diagnostics.Debug.WriteLine($"✅ ContenidoActual asignado: {ContenidoActual?.GetType().Name ?? "NULL"}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en AbrirReporte: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                
                MessageBox.Show(
                    $"Error al abrir reporte:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}