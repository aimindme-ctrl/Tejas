using System.Diagnostics;

namespace TejasCareConnect.Services;

public class ApiLauncher
{
    private static Process? _apiProcess;
    private static readonly string ApiUrl = "http://localhost:5088";
    
    public static async Task<bool> EnsureApiIsRunningAsync()
    {
        // Check if API is already running
        if (await IsApiRunningAsync())
        {
            Console.WriteLine("API is already running.");
            return true;
        }

        // Try to start the API
        Console.WriteLine("Starting Web API...");
        if (!StartApiProcess())
        {
            Console.WriteLine("Failed to start API process.");
            return false;
        }

        // Wait for API to be ready (max 90 seconds for build + startup)
        for (int i = 0; i < 180; i++)
        {
            await Task.Delay(500);
            
            if (i % 10 == 0) // Log every 5 seconds
            {
                Console.WriteLine($"Waiting for API... ({i * 0.5} seconds)");
            }
            
            if (await IsApiRunningAsync())
            {
                Console.WriteLine("API is now running and ready.");
                return true;
            }
        }

        Console.WriteLine("API did not start in time (waited 90 seconds).");
        return false;
    }

    private static bool StartApiProcess()
    {
        try
        {
            // Get the path to the Web API project - try multiple possible locations
            var appDirectory = AppContext.BaseDirectory;
            Console.WriteLine($"App directory: {appDirectory}");
            
            string? webApiPath = null;
            
            // Try relative path from bin directory
            var pathAttempts = new[]
            {
                Path.GetFullPath(Path.Combine(appDirectory, "..", "..", "..", "..", "..", "..", "TejasCareConnect.Web")),
                Path.GetFullPath(Path.Combine(appDirectory, "..", "..", "..", "..", "TejasCareConnect.Web")),
                Path.GetFullPath(Path.Combine(appDirectory, "..", "..", "TejasCareConnect.Web"))
            };

            foreach (var path in pathAttempts)
            {
                Console.WriteLine($"Checking path: {path}");
                if (Directory.Exists(path))
                {
                    webApiPath = path;
                    Console.WriteLine($"Found Web API at: {path}");
                    break;
                }
            }

            if (webApiPath == null)
            {
                Console.WriteLine("Web API directory not found in any expected location");
                return false;
            }

            // Use dotnet run which handles build automatically
            Console.WriteLine("Starting Web API with dotnet run...");
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --launch-profile http", // Explicitly use http profile on port 5088
                WorkingDirectory = webApiPath,
                UseShellExecute = true, // Changed to true to use system shell
                CreateNoWindow = false, // Show window for debugging
                WindowStyle = ProcessWindowStyle.Minimized
            };

            _apiProcess = Process.Start(startInfo);
            
            if (_apiProcess == null)
            {
                Console.WriteLine("Failed to create API process.");
                return false;
            }

            Console.WriteLine($"API process started with PID: {_apiProcess.Id}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception starting API: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
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
        if (_apiProcess != null && !_apiProcess.HasExited)
        {
            Console.WriteLine("Stopping API process...");
            _apiProcess.Kill(true);
            _apiProcess.Dispose();
            _apiProcess = null;
        }
    }
}
