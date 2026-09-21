using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace POS.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("  POS SALSAMENTARIA - UPDATER");
            Console.WriteLine("=================================");
            Console.WriteLine();

            if (args.Length < 2)
            {
                Console.WriteLine("Uso: POS.Updater.exe <ruta_actualizacion> <ruta_aplicacion>");
                return;
            }

            string updatePath = args[0];
            string appPath = args[1];

            try
            {
                Console.WriteLine("Esperando cierre de aplicación...");
                
                var processes = Process.GetProcessesByName("POS.UI");
                foreach (var p in processes)
                {
                    p.WaitForExit(10000); // Esperar hasta 10 segundos a que cierre
                }
                Thread.Sleep(1000); // Margen de seguridad adicional

                Console.WriteLine("Aplicando actualización...");
                
                // Copiar archivos nuevos
                CopyDirectory(updatePath, appPath, true);

                Console.WriteLine("Actualización aplicada exitosamente");
                Console.WriteLine();
                Console.WriteLine("Reiniciando aplicación...");
                Thread.Sleep(1000);

                // Reiniciar aplicación
                Process.Start(Path.Combine(appPath, "POS.UI.exe"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante actualización: {ex.Message}");
                Console.WriteLine("Presione cualquier tecla para salir...");
                Console.ReadKey();
            }
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Directorio no encontrado: {sourceDir}");

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
                Console.WriteLine($"  Copiado: {file.Name}");
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}