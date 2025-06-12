using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Mappers;
using WarsztatSamochodowyApp.Services;
using WarsztatSamochodowyApp.Services.Authorization;
using WarsztatSamochodowyApp.Services.Pdf;
using WarsztatSamochodowyApp.Services.Reports;

namespace WarsztatSamochodowyApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //CONTEXT do danych z bazy
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            //CONTEXT do tożsamości
            builder.Services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            //Dodawanie mapperow zeby dzialalo DI (jako singletony, prowadzacy bedzie wniebowziety)
            builder.Services.Scan(scan => scan
                .FromAssemblyOf<PartMapper>() // ← dowolny mapper jako punkt startowy
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Mapper")))
                .AsSelf()
                .WithSingletonLifetime());

            // Dodanie ról i polityk autoryzacji
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAuthorizationHandler, AssignedMechanicHandler>();
            builder.Services.AddAuthorization(options => { options.AddCustomAuthorizationPolicies(); });


            builder.Services.AddRazorPages();

            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

        builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

        builder.Services.AddScoped<IReportService, ReportService>();
        
        builder.Services.AddTransient<MonthlyRepairPdfExporter>();

        
        var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await RoleInitializer.InitializeAsync(services);
            }


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //DataSeeder.Seed(app);
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapRazorPages();

            app.MapStaticAssets();
            app.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unhandled exception in Program.Main");
            throw;
        }
        finally
        {
            LogManager.Shutdown();
        }
    }
}