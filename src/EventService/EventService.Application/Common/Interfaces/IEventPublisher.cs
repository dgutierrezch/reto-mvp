using EventPlatform.Contracts.Messages;

namespace EventService.Application.Common.Interfaces;

/// <summary>
/// Puerto de salida hacia el broker de mensajería. La implementación concreta
/// (MassTransit + RabbitMQ) vive en Infrastructure.
/// </summary>
public interface IEventPublisher
{
    Task PublishEventCreatedAsync(EventCreatedMessage message, CancellationToken cancellationToken = default);
}
