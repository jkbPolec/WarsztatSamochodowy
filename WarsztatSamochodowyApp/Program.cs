using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;

namespace WarsztatSamochodowyApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddDbContext<IdentityContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddDefaultIdentity<AppUser>(options => { options.SignIn.RequireConfirmedAccount = false; })
            .AddRoles<IdentityRole>().AddEntityFrameworkStores<IdentityContext>();

        builder.Services.AddRazorPages();

        var app = builder.Build();

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

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}