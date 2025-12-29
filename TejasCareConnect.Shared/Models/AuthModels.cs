namespace TejasCareConnect.Shared.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public UserDto? User { get; set; }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string RoleName => Role.ToString();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public int? PatientId { get; set; } // Link to patient record for patient portal users

    // Role-based permissions
    public bool CanManageUsers => Role == UserRole.Admin;
    public bool CanSubmitData => Role == UserRole.Admin || Role == UserRole.Contributor;
    public bool CanViewData => true; // All roles can view
}

public class ChangeRoleRequest
{
    public int UserId { get; set; }
    public UserRole NewRole { get; set; }
}

public class ResetUserPasswordRequest
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
