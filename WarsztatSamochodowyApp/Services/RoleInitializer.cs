using Microsoft.AspNetCore.Identity;
using WarsztatSamochodowyApp.Data;

namespace WarsztatSamochodowyApp.Services;

public static class RoleInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        string[] roles = { "Admin", "User", "Mechanik", "Recepcjonista" };

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

        // Dodanie dwóch mechaników
        var mechanik1Email = "m1@gmail.com";
        var mechanik2Email = "m2@gmail.com";
        var mechanikPassword = "Mechanik1!";

        var mechanik1 = await userManager.FindByEmailAsync(mechanik1Email);
        if (mechanik1 == null)
        {
            var newMechanik1 = new AppUser
            {
                UserName = mechanik1Email,
                Email = mechanik1Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newMechanik1, mechanikPassword);
            if (result.Succeeded) await userManager.AddToRoleAsync(newMechanik1, "Mechanik");
        }

        var mechanik2 = await userManager.FindByEmailAsync(mechanik2Email);
        if (mechanik2 == null)
        {
            var newMechanik2 = new AppUser
            {
                UserName = mechanik2Email,
                Email = mechanik2Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newMechanik2, mechanikPassword);
            if (result.Succeeded) await userManager.AddToRoleAsync(newMechanik2, "Mechanik");
        }

        // Dodanie recepcjonisty
        var recepcjonistaEmail = "r@gmail.com";
        var recepcjonistaPassword = "Recepcja1!";

        var recepcjonista = await userManager.FindByEmailAsync(recepcjonistaEmail);
        if (recepcjonista == null)
        {
            var newRecepcjonista = new AppUser
            {
                UserName = recepcjonistaEmail,
                Email = recepcjonistaEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newRecepcjonista, recepcjonistaPassword);
            if (result.Succeeded) await userManager.AddToRoleAsync(newRecepcjonista, "Recepcjonista");
        }
    }
}