using Microsoft.EntityFrameworkCore;
using NewProject.Data;
using NewProject.Models;
using NewProject.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Set port explicitly to 5008
builder.WebHost.UseUrls("http://localhost:5008");

// Add logging
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Developer exception filter
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Identity cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
});

// HttpClient for API communication
builder.Services.AddHttpClient("DiseaseAPI", client =>
{
    var apiBaseUrl = builder.Configuration.GetValue<string>("DetectionApi:BaseUrl") ?? "http://localhost:8000";
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add your custom services
builder.Services.AddScoped<IDiseaseDetectionService, DiseaseDetectionService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlaskAPI", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000", "http://127.0.0.1:5000", 
                "http://localhost:8000", "http://127.0.0.1:8000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    // Try to initialize database if it exists
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
            Console.WriteLine("Database created successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFlaskAPI");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Register all necessary routes
// Disease detection route
app.MapControllerRoute(
    name: "diseaseDetection",
    pattern: "Disease/Detection",
    defaults: new { controller = "Detection", action = "Index" });

// Heart disease routes
app.MapControllerRoute(
    name: "heart-disease",
    pattern: "HeartDisease/{action=Index}/{id?}",
    defaults: new { controller = "HeartDisease" });

app.MapControllerRoute(
    name: "heart-result",
    pattern: "HeartDisease/Result",
    defaults: new { controller = "HeartDisease", action = "Result" });

// Default route must be last
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Log startup information
Console.WriteLine($"Application running on: {string.Join(", ", app.Urls)}");

app.Run();