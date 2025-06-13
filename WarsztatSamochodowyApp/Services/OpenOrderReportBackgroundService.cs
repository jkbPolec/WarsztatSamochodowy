using Microsoft.AspNetCore.Identity;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.DTO;
using WarsztatSamochodowyApp.Services.Reports;

namespace WarsztatSamochodowyApp.Services;

using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class OpenOrderReportBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OpenOrderReportBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public OpenOrderReportBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OpenOrderReportBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

 
    private async Task<List<string>> GetAdminEmailsAsync(IServiceScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var users = await userManager.GetUsersInRoleAsync("Admin");
        return users
            .Select(u => u.Email)
            .Where(email => !string.IsNullOrEmpty(email))
            .ToList();
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

                var reportData = await reportService.GenerateCurrentServiceOrdersReportAsync();

                var pdfBytes = GeneratePdf(reportData);
                await SendEmailWithAttachmentAsync(pdfBytes, scope);

                _logger.LogInformation("Raport został wysłany: {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas generowania lub wysyłania raportu.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // zmień później na 24h
        }
    }

    private byte[] GeneratePdf(List<CurrentServiceOrderReportItemDto> data)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        using var stream = new MemoryStream();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Content()
                    .Column(col =>
                    {
                        col.Item().Text("📋 Otwarte zlecenia napraw").FontSize(18).Bold();

                        foreach (var item in data)
                        {
                            col.Item().Text($"#{item.OrderId} | {item.OrderDate:yyyy-MM-dd} | {item.VehicleIdentifier} | {item.MechanicFullName} | {item.Status}");
                        }
                    });
            });
        }).GeneratePdf(stream);

        return stream.ToArray(); // zwraca zawartość jako tablicę bajtów
    }




    private async Task SendEmailWithAttachmentAsync(byte[] pdfBytes, IServiceScope scope)
    {
        var adminEmails = await GetAdminEmailsAsync(scope);
        if (!adminEmails.Any())
        {
            _logger.LogWarning("Brak zarejestrowanych administratorów do wysłania raportu.");
            return;
        }

        var smtpSection = _configuration.GetSection("Smtp");

        var message = new MailMessage
        {
            From = new MailAddress(smtpSection["Sender"]!),
            Subject = "Raport otwartych napraw",
            Body = "W załączniku znajduje się automatyczny raport otwartych zleceń serwisowych.",
            IsBodyHtml = false
        };

        foreach (var email in adminEmails)
        {
            message.To.Add(email);
        }

        var attachment = new Attachment(new MemoryStream(pdfBytes), "open_orders.pdf", "application/pdf");
        message.Attachments.Add(attachment);

        using var client = new SmtpClient(smtpSection["Host"])
        {
            Port = int.Parse(smtpSection["Port"] ?? "587"),
            Credentials = new NetworkCredential(smtpSection["Username"], smtpSection["Password"]),
            EnableSsl = false
        };

        await client.SendMailAsync(message);
    }



}
