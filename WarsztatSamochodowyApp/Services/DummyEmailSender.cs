using Microsoft.AspNetCore.Identity.UI.Services;

namespace WarsztatSamochodowyApp.Services;

public class DummyEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Nic nie rób, totalny fake
        return Task.CompletedTask;
    }
}