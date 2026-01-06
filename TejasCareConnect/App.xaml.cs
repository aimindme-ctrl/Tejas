using TejasCareConnect.Services;

namespace TejasCareConnect;

public partial class App : Application
{
    private Window? _mainWindow;
    private LoadingPage? _loadingPage;

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _loadingPage = new LoadingPage();
        _mainWindow = new Window(_loadingPage) { Title = "TejasCareConnect" };

        // Start API in background
        Task.Run(async () =>
        {
            try
            {
                _loadingPage.UpdateStatus("Starting Web API (this may take 30-60 seconds)...");
                
                // Show progress updates
                var progressTask = Task.Run(async () =>
                {
                    for (int i = 0; i < 90; i++)
                    {
                        await Task.Delay(1000);
                        var seconds = i + 1;
                        _loadingPage.UpdateStatus($"Starting Web API... ({seconds} seconds)");
                    }
                });
                
                var apiReady = await ApiLauncher.EnsureApiIsRunningAsync();
                
                if (apiReady)
                {
                    _loadingPage.UpdateStatus("Web API ready! Loading application...");
                    await Task.Delay(500); // Brief pause
                    
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        if (_mainWindow != null)
                        {
                            _mainWindow.Page = new MainPage();
                        }
                    });
                }
                else
                {
                    _loadingPage.ShowError(
                        "Failed to start the Web API after 90 seconds.\n\n" +
                        "Please check:\n" +
                        "1. .NET SDK is installed\n" +
                        "2. Port 5088 is not in use\n" +
                        "3. Check the console window for errors\n" +
                        "4. Try manually starting the Web API first");
                }
            }
            catch (Exception ex)
            {
                _loadingPage.ShowError($"Error: {ex.Message}");
            }
        });

        return _mainWindow;
    }

    protected override void CleanUp()
    {
        ApiLauncher.StopApi();
        base.CleanUp();
    }
}
