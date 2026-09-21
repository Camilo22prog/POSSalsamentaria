using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var url = "https://github.com/Camilo22prog/Actualizaciones-Pos-Salsamentaria/releases/download/v1.1.0/POS_Update_1.1.0.zip";
        Console.WriteLine($"Starting check for: {url}\n");

        // Test 1: Standard HttpClient (no User-Agent)
        try
        {
            using var client = new HttpClient();
            Console.WriteLine("Test 1: Fetching WITHOUT User-Agent...");
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            Console.WriteLine($"Test 1 RESULT: {response.StatusCode} ({(int)response.StatusCode})");
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Test 1 SUCCESS!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test 1 FAILED! Exception: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            Console.WriteLine();
        }

        // Test 2: HttpClient WITH User-Agent
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            Console.WriteLine("Test 2: Fetching WITH User-Agent...");
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            Console.WriteLine($"Test 2 RESULT: {response.StatusCode} ({(int)response.StatusCode})");
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Test 2 SUCCESS!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test 2 FAILED! Exception: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            Console.WriteLine();
        }
    }
}
