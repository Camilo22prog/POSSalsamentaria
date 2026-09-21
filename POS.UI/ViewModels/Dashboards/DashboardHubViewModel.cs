using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using POS.UI.Services;
using POS.UI.Views.Dashboards;
using System;
using System.Windows.Controls;

namespace POS.UI.ViewModels.Dashboards
{
    public partial class DashboardHubViewModel : ObservableObject
    {
        private readonly IServiceProvider _services;

        [ObservableProperty]
        private UserControl? _contenidoActual;

        private string _tabActivo = "Principal";

        private DashboardPrincipalView? _principalView;
        private DashboardVentasView? _ventasView;
        private DashboardInventarioView? _inventarioView;
        private DashboardFinancieroView? _financieroView;

        public bool IsPrincipalActivo => _tabActivo == "Principal";
        public bool IsVentasActivo => _tabActivo == "Ventas";
        public bool IsInventarioActivo => _tabActivo == "Inventario";
        public bool IsFinancieroActivo => _tabActivo == "Financiero";

        // Visible solo para Administrador y Supervisor
        public bool PuedeVerFinanciero =>
            SessionService.Instance.UsuarioActual?.PuedeVerCostos ?? false;

        public DashboardHubViewModel(IServiceProvider services)
        {
            _services = services;
        }

        public void Initialize()
        {
            NavegarPrincipal();
        }

        private void SetActiveTab(string tab)
        {
            _tabActivo = tab;
            OnPropertyChanged(nameof(IsPrincipalActivo));
            OnPropertyChanged(nameof(IsVentasActivo));
            OnPropertyChanged(nameof(IsInventarioActivo));
            OnPropertyChanged(nameof(IsFinancieroActivo));
        }

        [RelayCommand]
        private void NavegarPrincipal()
        {
            _principalView ??= _services.GetRequiredService<DashboardPrincipalView>();
            ContenidoActual = _principalView;
            SetActiveTab("Principal");
        }

        [RelayCommand]
        private void NavegarVentas()
        {
            _ventasView ??= _services.GetRequiredService<DashboardVentasView>();
            ContenidoActual = _ventasView;
            SetActiveTab("Ventas");
        }

        [RelayCommand]
        private void NavegarInventario()
        {
            _inventarioView ??= _services.GetRequiredService<DashboardInventarioView>();
            ContenidoActual = _inventarioView;
            SetActiveTab("Inventario");
        }

        [RelayCommand]
        private void NavegarFinanciero()
        {
            _financieroView ??= _services.GetRequiredService<DashboardFinancieroView>();
            ContenidoActual = _financieroView;
            SetActiveTab("Financiero");
        }
    }
}
