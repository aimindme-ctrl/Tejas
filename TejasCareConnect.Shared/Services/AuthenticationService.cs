using System.Net.Http.Headers;
using System.Net.Http.Json;
using TejasCareConnect.Shared.Models;

namespace TejasCareConnect.Shared.Services;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<UserDto?> GetCurrentUserAsync();
    Task<string?> GetTokenAsync();
    Task<ApiResponse<bool>> ChangePasswordAsync(string currentPassword, string newPassword);
    UserDto? CurrentUser { get; }
    event EventHandler<UserDto?>? AuthenticationStateChanged;
}

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private UserDto? _currentUser;

    public UserDto? CurrentUser => _currentUser;
    public event EventHandler<UserDto?>? AuthenticationStateChanged;

    public AuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // Don't load from storage - require login every time
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                
                if (loginResponse?.Success == true && loginResponse.User != null)
                {
                    // Store in memory only, not in SecureStorage
                    _currentUser = loginResponse.User;
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                    AuthenticationStateChanged?.Invoke(this, _currentUser);
                }

                return loginResponse ?? new LoginResponse { Success = false, Message = "Login failed" };
            }

            var errorResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return errorResponse ?? new LoginResponse { Success = false, Message = "Login failed" };
        }
        catch (Exception ex)
        {
            return new LoginResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task LogoutAsync()
    {
        // Clear in-memory data only
        _currentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        AuthenticationStateChanged?.Invoke(this, null);
        await Task.CompletedTask;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        // Return in-memory user only
        await Task.CompletedTask;
        return _currentUser;
    }

    public async Task<string?> GetTokenAsync()
    {
        // No token persistence
        await Task.CompletedTask;
        return null;
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            if (_currentUser == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Not authenticated",
                    Data = false
                };
            }

            var request = new
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? new ApiResponse<bool> { Success = false, Message = "Failed to change password" };
            }

            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return errorResponse ?? new ApiResponse<bool> { Success = false, Message = "Failed to change password" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error: {ex.Message}",
                Data = false
            };
        }
    }
}
