using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string password = "admin";
        string hash = HashPassword(password);
        
        Console.WriteLine("Password: " + password);
        Console.WriteLine("Hash generado: " + hash);
        Console.WriteLine();
        Console.WriteLine("SQL para actualizar:");
        Console.WriteLine($"UPDATE Usuarios SET PasswordHash = '{hash}' WHERE NombreUsuario = 'admin';");
        Console.WriteLine();
        Console.ReadLine();
    }

    static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}