namespace TejasCareConnect.Shared.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int? PatientId { get; set; } // Link to patient record for patient portal users
}

public enum UserRole
{
    Viewer = 0,
    Contributor = 1,
    Admin = 2
}
