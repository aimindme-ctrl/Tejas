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

        // Check for API in background
        Task.Run(async () =>
        {
            try
            {
                _loadingPage.UpdateStatus("Checking for Web API...");
                
                var apiReady = await ApiLauncher.EnsureApiIsRunningAsync();
                
                if (apiReady)
                {
                    _loadingPage.UpdateStatus("Web API ready! Loading application...");
                    await Task.Delay(500);
                    
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
                        "Web API is not running!\n\n" +
                        "Please start the Web API first:\n\n" +
                        "1. Open PowerShell\n" +
                        "2. Navigate to: TejasCareConnect.Web\n" +
                        "3. Run: dotnet run --launch-profile http\n\n" +
                        "Or use the VS Code launch configuration.");
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
