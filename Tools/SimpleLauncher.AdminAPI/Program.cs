using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleLauncher.AdminAPI;
using SimpleLauncher.AdminAPI.Data;
using SimpleLauncher.AdminAPI.Middleware;
using SimpleLauncher.AdminAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=simplelauncher.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Add ASP.NET Core Identity for authentication
builder.Services.AddDefaultIdentity<IdentityUser>(static options => { options.SignIn.RequireConfirmedAccount = false; })
    .AddRoles<IdentityRole>() // Add role support
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();
builder.Services.AddRazorPages(); // Add Razor Pages for the admin UI

// Swagger/OpenAPI for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClientFactory for making HTTP requests
builder.Services.AddHttpClient();

// Register the custom bug report service
builder.Services.AddScoped<IBugReportService, BugReportService>();

var app = builder.Build();

// 2. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add the global exception handler middleware early in the pipeline
app.UseMiddleware<GlobalExceptionHandler>();

app.UseStaticFiles(); // Add this line to serve static files

app.UseHttpsRedirection();

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages(); // Map Razor Pages endpoints

// 3. Seed the database with an admin user and role
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        Log.DatabaseSeedingError(logger, ex);
    }
}

app.Run();

