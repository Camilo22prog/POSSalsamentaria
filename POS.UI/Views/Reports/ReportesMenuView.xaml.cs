using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using POS.UI.ViewModels.Reports;
namespace POS.UI.Views.Reports
{
    public partial class ReportesMenuView : UserControl
    {
        private readonly ReportesMenuViewModel _viewModel;

        public ReportesMenuView(ReportesMenuViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;
            
            System.Diagnostics.Debug.WriteLine("✅ ReportesMenuView creado");
        }

        private void BtnVentasPeriodo_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("🖱️ Click en Ventas por Período");
            
            try
            {
                var vista = App.Services.GetRequiredService<VentasPorPeriodoView>();
                System.Diagnostics.Debug.WriteLine($"📊 Vista creada: {vista != null}");
                
                ContenidoControl.Content = vista;
                System.Diagnostics.Debug.WriteLine("✅ Contenido asignado");
                
                MenuPanel.Visibility = Visibility.Collapsed;
                ContenidoPanel.Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine("✅ Visibilidad cambiada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVentasCliente_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("🖱️ Click en Ventas por Cliente");
            
            try
            {
                var vista = App.Services.GetRequiredService<VentasPorClienteView>();
                ContenidoControl.Content = vista;
                MenuPanel.Visibility = Visibility.Collapsed;
                ContenidoPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVentasProducto_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("🖱️ Click en Ventas por Producto");
            
            try
            {
                var vista = App.Services.GetRequiredService<VentasPorProductoView>();
                ContenidoControl.Content = vista;
                MenuPanel.Visibility = Visibility.Collapsed;
                ContenidoPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVentasMetodoPago_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vista = App.Services.GetRequiredService<VentasPorMetodoPagoView>();
                ContenidoControl.Content = vista;
                MenuPanel.Visibility = Visibility.Collapsed;
                ContenidoPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEstadoResultados_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vista = App.Services.GetRequiredService<EstadoResultadosView>();
                ContenidoControl.Content = vista;
                MenuPanel.Visibility = Visibility.Collapsed;
                ContenidoPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vista = App.Services.GetRequiredService<ReporteInventarioView>();
                ContenidoControl.Content = vista;
                MenuPanel.Visibility = Visibility.Collapsed;
                ContenidoPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAnalisisABC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vista = App.Services.GetRequiredService<AnalisisABCView>();
                ContenidoControl.Content = vista;
                MenuPanel.Visibility = Visibility.Collapsed;
                ContenidoPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDesempenoUsuarios_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vista = App.Services.GetRequiredService<DesempenoUsuarioView>();
                ContenidoControl.Content = vista;
                MenuPanel.Visibility = Visibility.Collapsed;
                ContenidoPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVentasAnuladas_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vista = App.Services.GetRequiredService<VentasAnuladasView>();
                ContenidoControl.Content = vista;
                MenuPanel.Visibility = Visibility.Collapsed;
                ContenidoPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnReporteArqueos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vista = App.Services.GetRequiredService<ReporteArqueosView>();
                ContenidoControl.Content = vista;
                MenuPanel.Visibility = Visibility.Collapsed;
                ContenidoPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("⬅ Volver al menú");
            ContenidoControl.Content = null;
            MenuPanel.Visibility = Visibility.Visible;
            ContenidoPanel.Visibility = Visibility.Collapsed;
        }
    }
}