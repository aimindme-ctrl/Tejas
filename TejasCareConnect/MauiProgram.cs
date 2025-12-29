using Microsoft.Extensions.Logging;
using TejasCareConnect.Shared.Services;
using TejasCareConnect.Services;
using Microsoft.AspNetCore.Components.Authorization;
using TejasCareConnect.Shared.Components;

namespace TejasCareConnect;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the TejasCareConnect.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        // Configure HttpClient with base address
        builder.Services.AddScoped(sp => new HttpClient 
        { 
            BaseAddress = new Uri("http://localhost:5088/") // Update with your API URL
        });

        // Add authentication and user services
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IPatientService, PatientService>();
        
        // Add authorization services
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
