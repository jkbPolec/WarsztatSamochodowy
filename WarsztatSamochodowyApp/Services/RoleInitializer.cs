using Microsoft.AspNetCore.Identity;
using WarsztatSamochodowyApp.Data;

namespace WarsztatSamochodowyApp.Services;

public static class RoleInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        string[] roles = { "Admin", "User" };

        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // Dodanie admina jeśli nie istnieje
        var adminEmail = "admin@gmail.com";
        var adminPassword = "Admin1!";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            var newAdmin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newAdmin, adminPassword);
            if (result.Succeeded) await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}