using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Services;

namespace WarsztatSamochodowyApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


        builder.Services.AddDbContext<IdentityContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


        builder.Services.AddRazorPages();

        builder.Services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddTransient<IEmailSender, DummyEmailSender>();


        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await RoleInitializer.InitializeAsync(services);
        }


        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRazorPages();


        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();


        app.MapStaticAssets();
        app.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}