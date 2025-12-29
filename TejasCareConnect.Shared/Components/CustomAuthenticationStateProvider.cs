using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TejasCareConnect.Shared.Services;

namespace TejasCareConnect.Shared.Components;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAuthenticationService _authService;

    public CustomAuthenticationStateProvider(IAuthenticationService authService)
    {
        _authService = authService;
        _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };

                var identity = new ClaimsIdentity(claims, "jwt");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                return new AuthenticationState(claimsPrincipal);
            }
        }
        catch (Exception)
        {
            // If there's an error loading auth state, return anonymous
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private void OnAuthenticationStateChanged(object? sender, Models.UserDto? user)
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
