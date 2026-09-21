using CommunityToolkit.Mvvm.ComponentModel;

namespace POS.UI.Models
{
    public partial class ItemCarrito : ObservableObject
    {
        [ObservableProperty]
        private int _productoId;

        [ObservableProperty]
        private string _codigo = string.Empty;

        [ObservableProperty]
        private string _nombre = string.Empty;

        [ObservableProperty]
        private decimal _precioUnitario; // YA incluye IVA

        [ObservableProperty]
        private decimal _cantidad;

        [ObservableProperty]
        private decimal _porcentajeIVA; // Porcentaje para informar

        [ObservableProperty]
        private decimal _descuento;

        [ObservableProperty]
        private decimal _subtotal; // Precio × Cantidad - Descuento (CON IVA)

        [ObservableProperty]
        private decimal _montoIVA; // IVA incluido en el subtotal

        [ObservableProperty]
        private decimal _total; // = Subtotal (porque IVA ya está incluido)

        [ObservableProperty]
        private decimal _costoUnitario;

        public void CalcularTotales()
        {
            // Subtotal = Precio × Cantidad - Descuento (CON IVA incluido)
            Subtotal = Math.Round((PrecioUnitario * Cantidad) - Descuento, 0, MidpointRounding.AwayFromZero);

            // Calcular IVA que está INCLUIDO en el subtotal
            // Fórmula: Subtotal / (1 + %IVA) × %IVA
            if (PorcentajeIVA > 0)
            {
                var factorIVA = 1 + (PorcentajeIVA / 100);
                MontoIVA = Subtotal - (Subtotal / factorIVA);
            }
            else
            {
                MontoIVA = 0;
            }

            // Total = Subtotal (porque el IVA ya está incluido)
            Total = Subtotal;
        }
    }
}