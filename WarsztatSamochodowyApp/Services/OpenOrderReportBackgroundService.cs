using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
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

    public OpenOrderReportBackgroundService(IServiceProvider serviceProvider, ILogger<OpenOrderReportBackgroundService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
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

                string pdfPath = "open_orders.pdf";
                GeneratePdf(reportData, pdfPath);

                await SendEmailWithAttachmentAsync(pdfPath);

                _logger.LogInformation("Raport został wysłany: {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas generowania lub wysyłania raportu.");
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // zmień na 24h po testach
        }
    }

    private void GeneratePdf(List<CurrentServiceOrderReportItemDto> data, string outputPath)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        using var stream = File.Create(outputPath);

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
    }



    private async Task SendEmailWithAttachmentAsync(string filePath)
    {
        var adminEmail = _configuration["adminEmail"] ?? "admin@example.com";
        var smtpSection = _configuration.GetSection("Smtp");

        var message = new MailMessage
        {
            From = new MailAddress(smtpSection["Sender"]!),
            Subject = "Raport otwartych napraw",
            Body = "W załączniku znajduje się automatyczny raport otwartych zleceń serwisowych.",
            IsBodyHtml = false
        };

        message.To.Add(adminEmail);
        message.Attachments.Add(new Attachment(filePath));

        using var client = new SmtpClient(smtpSection["Host"])
        {
            Port = int.Parse(smtpSection["Port"] ?? "587"),
            Credentials = new NetworkCredential(smtpSection["Username"], smtpSection["Password"]),
            EnableSsl = false
        };

        await client.SendMailAsync(message);
    }
}
