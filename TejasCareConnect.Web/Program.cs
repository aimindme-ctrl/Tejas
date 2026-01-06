using TejasCareConnect.Web.Components;
using TejasCareConnect.Shared.Services;
using TejasCareConnect.Web.Services;
using TejasCareConnect.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add authentication services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

// Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Add API controllers
builder.Services.AddControllers();

// Add device-specific services used by the TejasCareConnect.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// Add CORS for MAUI app
builder.Services.AddCors(options =>
{
    options.AddPolicy("MauiApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database - Only add PatientId column if upgrading from older version
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Ensure database exists (will not recreate if it already exists)
    // Comment out the following line if you want manual database creation:
    // db.Database.EnsureCreated();
    
    // Add PatientId column if it doesn't exist (for backward compatibility)
    try
    {
        await db.Database.ExecuteSqlRawAsync(
            @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PatientId')
              BEGIN
                  ALTER TABLE Users ADD PatientId INT NULL
              END");
    }
    catch { /* Column already exists */ }
    
    // Database seeding disabled - database is already populated
    // Uncomment the following lines if you need to seed a new database:
    // await PatientSeeder.SeedPatientsAsync(db);
    // await SeedPatientUsersAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("MauiApp");

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapStaticAssets();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(TejasCareConnect.Shared._Imports).Assembly);

app.Run();

async Task SeedPatientUsersAsync(ApplicationDbContext db)
{
    // Check if patient users already exist
    if (await db.Users.AnyAsync(u => u.PatientId.HasValue))
    {
        return; // Already seeded
    }

    // Get first 5 patients to create portal accounts for
    var patients = await db.Patients.OrderBy(p => p.Id).Take(5).ToListAsync();
    
    foreach (var patient in patients)
    {
        var patientUser = new TejasCareConnect.Shared.Models.User
        {
            Email = patient.Email.ToLower(), // Use patient's email
            FullName = patient.FirstName + " " + patient.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient123!"), // Default password
            Role = TejasCareConnect.Shared.Models.UserRole.Viewer,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            PatientId = patient.Id
        };

        // Check if user with this email already exists
        if (!await db.Users.AnyAsync(u => u.Email == patientUser.Email))
        {
            db.Users.Add(patientUser);
        }
    }

    await db.SaveChangesAsync();
    Console.WriteLine($"Seeded {patients.Count} patient portal users with PatientId links");
}
