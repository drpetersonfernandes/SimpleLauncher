using Microsoft.AspNetCore.Identity;

namespace SimpleLauncher.AdminAPI.Data;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        const string adminRole = "Admin";
        const string adminEmail = "admin@simplelauncher.com";
        // WARNING: Use a strong password and manage it securely (e.g., via user secrets).
        const string adminPassword = "Password123!";

        // Ensure the Admin role exists
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        // Ensure the Admin user exists
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                // Assign the Admin role to the new user
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }
}