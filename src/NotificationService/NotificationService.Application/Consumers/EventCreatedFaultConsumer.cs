using EventPlatform.Contracts.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Common.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Consumers;

/// <summary>
/// Se activa cuando EventCreatedConsumer agotó los reintentos configurados.
/// Registra el estado "Failed" para trazabilidad; el mensaje original
/// ya fue movido por MassTransit a la cola _error (DLQ).
/// </summary>
public sealed class EventCreatedFaultConsumer : IConsumer<Fault<EventCreatedMessage>>
{
    private readonly INotificationDbContext _db;
    private readonly ILogger<EventCreatedFaultConsumer> _logger;

    public EventCreatedFaultConsumer(INotificationDbContext db, ILogger<EventCreatedFaultConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Fault<EventCreatedMessage>> context)
    {
        var message = context.Message.Message;
        var errorMessage = context.Message.Exceptions.FirstOrDefault()?.Message ?? "Error desconocido";

        var alreadyLogged = await _db.NotificationLogs
            .AnyAsync(n => n.MessageId == message.MessageId, context.CancellationToken);

        if (alreadyLogged) return;

        var payloadHash = PayloadHasher.ComputeHash(message);

        var log = NotificationLog.CreateFailed(
            message.MessageId, message.EventId, message.Name, message.OccurredAt,
            message.CorrelationId, payloadHash, errorMessage);

        _db.NotificationLogs.Add(log);
        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogError(
            "EventCreated {MessageId} agotó reintentos y fue movido a DLQ. Motivo: {Error}",
            message.MessageId, errorMessage);
    }
}
