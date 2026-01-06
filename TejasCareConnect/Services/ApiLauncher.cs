using System.Diagnostics;

namespace TejasCareConnect.Services;

public class ApiLauncher
{
    private static readonly string ApiUrl = "http://localhost:5088";
    
    public static async Task<bool> EnsureApiIsRunningAsync()
    {
        // Check if API is already running
        Console.WriteLine("Checking if Web API is running...");
        
        for (int i = 0; i < 10; i++)
        {
            if (await IsApiRunningAsync())
            {
                Console.WriteLine("API is running and ready.");
                return true;
            }
            await Task.Delay(500);
        }

        Console.WriteLine("Web API is not running on http://localhost:5088");
        return false;
    }

    private static async Task<bool> IsApiRunningAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetAsync($"{ApiUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static void StopApi()
    {
        // No longer managing API process
    }
}
