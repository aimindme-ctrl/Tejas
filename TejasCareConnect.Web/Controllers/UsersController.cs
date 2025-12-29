using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TejasCareConnect.Shared.Models;
using TejasCareConnect.Web.Services;

namespace TejasCareConnect.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    {
        var response = await _userManagementService.GetAllUsersAsync();
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(int id)
    {
        var response = await _userManagementService.GetUserByIdAsync(id);
        
        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPut("change-role")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> ChangeUserRole([FromBody] ChangeRoleRequest request)
    {
        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var response = await _userManagementService.ChangeUserRoleAsync(adminUserId, request);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPut("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleUserActiveStatus(int id)
    {
        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var response = await _userManagementService.ToggleUserActiveStatusAsync(adminUserId, id);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
    {
        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var response = await _userManagementService.DeleteUserAsync(adminUserId, id);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
