using System.Net.Http.Json;
using TejasCareConnect.Shared.Models;

namespace TejasCareConnect.Shared.Services;

public interface IUserService
{
    Task<ApiResponse<List<UserDto>>> GetAllUsersAsync();
    Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId);
    Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterRequest request);
    Task<ApiResponse<UserDto>> ChangeUserRoleAsync(int userId, UserRole newRole);
    Task<ApiResponse<bool>> ToggleUserActiveStatusAsync(int userId);
    Task<ApiResponse<bool>> DeleteUserAsync(int userId);
    Task<ApiResponse<bool>> ResetUserPasswordAsync(int userId, string newPassword);
}

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;

    public UserService(HttpClient httpClient, IAuthenticationService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<UserDto>>>("api/auth/users");
            return response ?? new ApiResponse<List<UserDto>> { Success = false, Message = "Failed to retrieve users" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<UserDto>> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserDto>>($"api/users/{userId}");
            return response ?? new ApiResponse<UserDto> { Success = false, Message = "Failed to retrieve user" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserDto> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>() 
                    ?? new ApiResponse<UserDto> { Success = false, Message = "Failed to register user" };
            }

            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
            return errorResponse ?? new ApiResponse<UserDto> { Success = false, Message = "Failed to register user" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserDto> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<UserDto>> ChangeUserRoleAsync(int userId, UserRole newRole)
    {
        try
        {
            var request = new ChangeRoleRequest { UserId = userId, NewRole = newRole };
            var response = await _httpClient.PutAsJsonAsync("api/users/change-role", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>() 
                    ?? new ApiResponse<UserDto> { Success = false, Message = "Failed to change user role" };
            }

            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
            return errorResponse ?? new ApiResponse<UserDto> { Success = false, Message = "Failed to change user role" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserDto> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<bool>> ToggleUserActiveStatusAsync(int userId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/users/{userId}/toggle-active", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
                    ?? new ApiResponse<bool> { Success = false, Message = "Failed to toggle user status" };
            }

            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return errorResponse ?? new ApiResponse<bool> { Success = false, Message = "Failed to toggle user status" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(int userId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/users/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
                    ?? new ApiResponse<bool> { Success = false, Message = "Failed to delete user" };
            }

            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return errorResponse ?? new ApiResponse<bool> { Success = false, Message = "Failed to delete user" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<bool>> ResetUserPasswordAsync(int userId, string newPassword)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var request = new ResetUserPasswordRequest
            {
                UserId = userId,
                NewPassword = newPassword
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/reset-user-password", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
                    ?? new ApiResponse<bool> { Success = false, Message = "Failed to reset password" };
            }

            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return errorResponse ?? new ApiResponse<bool> { Success = false, Message = "Failed to reset password" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Success = false, Message = $"Error: {ex.Message}" };
        }
    }
}
