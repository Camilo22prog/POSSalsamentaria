using Microsoft.Data.SqlClient;
using System;
using System.Windows;

namespace POS.UI
{
    public class TestConnection
    {
        public static void Test()
        {
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
            
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show(
                        $"✅ Conexión exitosa a SQL Server!\n\nVersión: {connection.ServerVersion}",
                        "Test de Conexión",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error de conexión:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}