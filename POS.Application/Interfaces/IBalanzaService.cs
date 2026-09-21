namespace POS.Application.Interfaces
{
    public interface IBalanzaService
    {
        /// <summary>
        /// Conecta con la balanza en el puerto especificado
        /// </summary>
        Task<bool> ConectarAsync(string puerto, int baudRate = 9600);
        
        /// <summary>
        /// Lee el peso actual de la balanza
        /// </summary>
        Task<decimal> LeerPesoAsync();
        
        /// <summary>
        /// Detecta automáticamente el puerto de la balanza
        /// </summary>
        Task<string?> DetectarPuertoAsync();
        
        /// <summary>
        /// Verifica si la balanza está conectada
        /// </summary>
        bool EstaConectada { get; }
        
        /// <summary>
        /// Desconecta la balanza
        /// </summary>
        void Desconectar();
        
        /// <summary>
        /// Obtiene la lista de puertos COM disponibles
        /// </summary>
        string[] ObtenerPuertosDisponibles();
    }
}