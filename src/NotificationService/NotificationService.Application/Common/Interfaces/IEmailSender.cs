namespace NotificationService.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEventCreatedEmailAsync(string eventName, DateTime occurredAt, CancellationToken cancellationToken = default);
}
