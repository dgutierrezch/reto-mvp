namespace EventPlatform.Contracts.Messages;

/// <summary>
/// Contrato de mensaje publicado por EventService cuando se crea un evento.
/// Es el "schema" compartido entre productor y consumidor.
/// </summary>
public sealed record EventCreatedMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public Guid EventId { get; init; }
    public string Name { get; init; } = default!;
    public DateTime OccurredAt { get; init; }
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public int Version { get; init; } = 1;
}
