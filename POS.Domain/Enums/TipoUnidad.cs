namespace POS.Domain.Enums
{
    public enum TipoUnidad
    {
        Unidad = 1,      // Venta por unidad (ej: 1 cerveza)
        Peso = 2,        // Venta por peso (ej: 0.5 kg de jamón)
        Ambos = 3        // Puede venderse por ambos (ej: queso)
    }
}