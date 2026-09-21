using POS.Application.Services;
using System;
using System.Threading.Tasks;

namespace POS.UI
{
    public class TestBalanza
    {
        public static async Task Probar()
        {
            var balanza = new BalanzaService();
            
            Console.WriteLine("=== PRUEBA DE BALANZA ===\n");
            
            // 1. Ver puertos disponibles
            var puertos = balanza.ObtenerPuertosDisponibles();
            Console.WriteLine($"📍 Puertos COM disponibles: {string.Join(", ", puertos)}");
            
            if (puertos.Length == 0)
            {
                Console.WriteLine("❌ ERROR: No hay puertos COM en este PC");
                Console.WriteLine("💡 Conecta la balanza y verifica en Administrador de Dispositivos");
                return;
            }
            
            // 2. Probar COM3 específicamente
            Console.WriteLine($"\n🔌 Intentando conectar a COM3...");
            var resultado = await balanza.ConectarAsync("COM3");
            Console.WriteLine($"Resultado: {(resultado ? "✅ CONECTADO" : "❌ NO CONECTADO")}");
            Console.WriteLine($"Estado: {(balanza.EstaConectada ? "✅ ABIERTO" : "❌ CERRADO")}");
            
            if (balanza.EstaConectada)
            {
                Console.WriteLine("\n📊 Intentando leer peso (3 intentos)...\n");
                
                for (int i = 1; i <= 3; i++)
                {
                    Console.WriteLine($"--- Intento {i} ---");
                    var peso = await balanza.LeerPesoAsync();
                    Console.WriteLine($"⚖️ Peso leído: {peso} kg");
                    await Task.Delay(1500);
                }
                
                balanza.Desconectar();
                Console.WriteLine("\n🔌 Balanza desconectada");
            }
            
            Console.WriteLine("\n=== FIN DE PRUEBA ===");
        }
    }
}