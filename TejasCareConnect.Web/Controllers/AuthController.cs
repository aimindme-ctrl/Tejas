using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TejasCareConnect.Shared.Models;
using TejasCareConnect.Web.Services;

namespace TejasCareConnect.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        
        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var response = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("reset-user-password")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetUserPassword([FromBody] ResetUserPasswordRequest request)
    {
        var response = await _authService.ResetUserPasswordAsync(request.UserId, request.NewPassword);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    {
        var response = await _authService.GetAllUsersAsync();
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var fullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var role = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role) ?? "Viewer");

        var userDto = new UserDto
        {
            Id = userId,
            Email = email,
            FullName = fullName,
            Role = role,
            IsActive = true
        };

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Message = "User retrieved successfully",
            Data = userDto
        });
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
