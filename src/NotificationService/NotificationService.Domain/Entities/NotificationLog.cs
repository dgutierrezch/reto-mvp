using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Registro persistido por cada mensaje EventCreated procesado.
/// El índice único sobre MessageId es lo que garantiza idempotencia:
/// un mismo mensaje entregado dos veces por el broker no genera dos notificaciones.
/// </summary>
public sealed class NotificationLog
{
    public Guid Id { get; private set; }
    public Guid MessageId { get; private set; }
    public Guid EventId { get; private set; }
    public string EventName { get; private set; } = default!;
    public DateTime OccurredAt { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string PayloadHash { get; private set; } = default!;
    public NotificationStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    private NotificationLog() { } // EF Core

    public static NotificationLog CreateProcessed(
        Guid messageId, Guid eventId, string eventName, DateTime occurredAt, Guid correlationId, string payloadHash)
        => new()
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            EventId = eventId,
            EventName = eventName,
            OccurredAt = occurredAt,
            CorrelationId = correlationId,
            PayloadHash = payloadHash,
            Status = NotificationStatus.Processed,
            ProcessedAt = DateTime.UtcNow
        };

    public static NotificationLog CreateFailed(
        Guid messageId, Guid eventId, string eventName, DateTime occurredAt, Guid correlationId, string payloadHash, string error)
        => new()
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            EventId = eventId,
            EventName = eventName,
            OccurredAt = occurredAt,
            CorrelationId = correlationId,
            PayloadHash = payloadHash,
            Status = NotificationStatus.Failed,
            ErrorMessage = error,
            ProcessedAt = DateTime.UtcNow
        };
}
