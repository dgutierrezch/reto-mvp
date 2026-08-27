using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using NotificationService.Application.Common.Interfaces;

namespace NotificationService.Infrastructure.Email;

/// <summary>
/// Envía el correo vía SMTP usando MailKit. En docker-compose apunta a MailHog
/// (captura correos localmente sin necesitar credenciales reales) para la demo.
/// </summary>
public sealed class MailKitEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IConfiguration configuration, ILogger<MailKitEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEventCreatedEmailAsync(string eventName, DateTime occurredAt, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_configuration["Smtp:From"] ?? "no-reply@event-platform.local"));
        message.To.Add(MailboxAddress.Parse(_configuration["Smtp:DemoRecipient"] ?? "demo@event-platform.local"));
        message.Subject = $"Nuevo evento creado: {eventName}";
        message.Body = new TextPart("plain")
        {
            Text = $"Se creó el evento '{eventName}' el {occurredAt:u}."
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["Smtp:Host"] ?? "mailhog",
            int.Parse(_configuration["Smtp:Port"] ?? "1025"),
            MailKit.Security.SecureSocketOptions.None,
            cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Correo de EventCreated enviado para el evento {EventName}", eventName);
    }
}
