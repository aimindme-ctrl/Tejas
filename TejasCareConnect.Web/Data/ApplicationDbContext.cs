using Microsoft.EntityFrameworkCore;
using TejasCareConnect.Shared.Models;

namespace TejasCareConnect.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MRN).IsUnique();
            entity.Property(e => e.MRN).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.BloodType).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        // Seed default admin user (password: Admin123!)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "admin@tejascare.com",
                FullName = "System Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );
    }
}
