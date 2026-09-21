using CommunityToolkit.Mvvm.ComponentModel;

namespace POS.UI.Models
{
    public partial class DenominacionItem : ObservableObject
    {
        [ObservableProperty]
        private string _nombre = string.Empty;

        [ObservableProperty]
        private int _valor;

        [ObservableProperty]
        private int _cantidad;

        [ObservableProperty]
        private decimal _total;

        partial void OnCantidadChanged(int value)
        {
            Total = Valor * value;
        }
    }
}