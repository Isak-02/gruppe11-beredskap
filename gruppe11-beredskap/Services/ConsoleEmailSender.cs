using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace gruppe11_beredskap.Services;

// "Sender" e-post ved å skrive den i Run-vinduet (loggen) i stedet for å sende den.
// Brukes for lenken til "Glemt passord" mens vi ikke har ekte e-postsending.
public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation(
            "E-post til {Epost}. Emne: {Emne}. Innhold: {Innhold}",
            email,
            subject,
            WebUtility.HtmlDecode(htmlMessage));
        return Task.CompletedTask;
    }
}
