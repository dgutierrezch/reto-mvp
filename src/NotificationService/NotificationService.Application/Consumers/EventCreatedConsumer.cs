using EventPlatform.Contracts.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Common.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Consumers;

/// <summary>
/// Consume EventCreated, garantiza idempotencia por MessageId, y envía el correo.
/// Si falla, relanza la excepción para que MassTransit aplique su política de
/// reintentos (configurada en Infrastructure); tras agotarlos, el mensaje
/// original va a la cola de error (_error) y EventCreatedFaultConsumer
/// registra el estado Failed.
/// </summary>
public sealed class EventCreatedConsumer : IConsumer<EventCreatedMessage>
{
    private readonly INotificationDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EventCreatedConsumer> _logger;

    public EventCreatedConsumer(INotificationDbContext db, IEmailSender emailSender, ILogger<EventCreatedConsumer> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EventCreatedMessage> context)
    {
        var message = context.Message;

        var alreadyProcessed = await _db.NotificationLogs
            .AnyAsync(n => n.MessageId == message.MessageId, context.CancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogInformation(
                "MessageId {MessageId} ya fue procesado previamente; se omite (idempotencia).",
                message.MessageId);
            return;
        }

        var payloadHash = PayloadHasher.ComputeHash(message);

        await _emailSender.SendEventCreatedEmailAsync(message.Name, message.OccurredAt, context.CancellationToken);

        var log = NotificationLog.CreateProcessed(
            message.MessageId, message.EventId, message.Name, message.OccurredAt, message.CorrelationId, payloadHash);

        _db.NotificationLogs.Add(log);
        await _db.SaveChangesAsync(context.CancellationToken);
    }
}
