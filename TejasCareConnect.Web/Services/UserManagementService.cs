using Microsoft.EntityFrameworkCore;
using TejasCareConnect.Shared.Models;
using TejasCareConnect.Web.Data;

namespace TejasCareConnect.Web.Services;

public interface IUserManagementService
{
    Task<ApiResponse<List<UserDto>>> GetAllUsersAsync();
    Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId);
    Task<ApiResponse<UserDto>> ChangeUserRoleAsync(int adminUserId, ChangeRoleRequest request);
    Task<ApiResponse<bool>> ToggleUserActiveStatusAsync(int adminUserId, int userId);
    Task<ApiResponse<bool>> DeleteUserAsync(int adminUserId, int userId);
}

public class UserManagementService : IUserManagementService
{
    private readonly ApplicationDbContext _context;

    public UserManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var userDtos = users.Select(MapToDto).ToList();

        return new ApiResponse<List<UserDto>>
        {
            Success = true,
            Message = $"Retrieved {userDtos.Count} users",
            Data = userDtos
        };
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "User not found"
            };
        }

        return new ApiResponse<UserDto>
        {
            Success = true,
            Message = "User retrieved successfully",
            Data = MapToDto(user)
        };
    }

    public async Task<ApiResponse<UserDto>> ChangeUserRoleAsync(int adminUserId, ChangeRoleRequest request)
    {
        var admin = await _context.Users.FindAsync(adminUserId);
        if (admin == null || admin.Role != UserRole.Admin)
        {
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Unauthorized: Admin privileges required"
            };
        }

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "User not found"
            };
        }

        // Prevent changing the role of the last admin
        if (user.Role == UserRole.Admin && request.NewRole != UserRole.Admin)
        {
            var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Cannot change role: At least one admin must remain"
                };
            }
        }

        user.Role = request.NewRole;
        await _context.SaveChangesAsync();

        return new ApiResponse<UserDto>
        {
            Success = true,
            Message = $"User role changed to {request.NewRole}",
            Data = MapToDto(user)
        };
    }

    public async Task<ApiResponse<bool>> ToggleUserActiveStatusAsync(int adminUserId, int userId)
    {
        var admin = await _context.Users.FindAsync(adminUserId);
        if (admin == null || admin.Role != UserRole.Admin)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Unauthorized: Admin privileges required"
            };
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "User not found"
            };
        }

        // Prevent deactivating the last admin
        if (user.Role == UserRole.Admin && user.IsActive)
        {
            var activeAdminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin && u.IsActive);
            if (activeAdminCount <= 1)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot deactivate: At least one active admin must remain"
                };
            }
        }

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully",
            Data = true
        };
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(int adminUserId, int userId)
    {
        var admin = await _context.Users.FindAsync(adminUserId);
        if (admin == null || admin.Role != UserRole.Admin)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Unauthorized: Admin privileges required"
            };
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "User not found"
            };
        }

        // Prevent deleting the last admin
        if (user.Role == UserRole.Admin)
        {
            var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete: At least one admin must remain"
                };
            }
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "User deleted successfully",
            Data = true
        };
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive
        };
    }
}
