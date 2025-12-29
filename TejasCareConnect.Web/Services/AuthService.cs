using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TejasCareConnect.Shared.Models;

namespace TejasCareConnect.Web.Services;

public interface IAuthService
{
    string GenerateJwtToken(User user);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<ApiResponse<bool>> ResetUserPasswordAsync(int userId, string newPassword);
    Task<ApiResponse<List<UserDto>>> GetAllUsersAsync();
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly Data.ApplicationDbContext _context;

    public AuthService(IConfiguration configuration, Data.ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public string GenerateJwtToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        // Add PatientId claim if user is linked to a patient record
        if (user.PatientId.HasValue)
        {
            claims.Add(new Claim("PatientId", user.PatientId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiryInHours"] ?? "24")),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (user == null || !user.IsActive)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        
        return new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            User = MapToDto(user)
        };
    }

    public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Email already registered"
            };
        }

        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new ApiResponse<UserDto>
        {
            Success = true,
            Message = "User registered successfully",
            Data = MapToDto(user)
        };
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "User not found"
            };
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Current password is incorrect"
            };
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Password changed successfully",
            Data = true
        };
    }
    public async Task<ApiResponse<bool>> ResetUserPasswordAsync(int userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "User not found",
                Data = false
            };
        }

        // Validate new password
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Password must be at least 6 characters",
                Data = false
            };
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Password reset successfully",
            Data = true
        };
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var userDtos = users.Select(MapToDto).ToList();

        return new ApiResponse<List<UserDto>>
        {
            Success = true,
            Message = "Users retrieved successfully",
            Data = userDtos
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
            IsActive = user.IsActive,
            PatientId = user.PatientId
        };
    }
}
