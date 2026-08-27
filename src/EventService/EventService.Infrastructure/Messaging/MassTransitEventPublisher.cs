using EventPlatform.Contracts.Messages;
using EventService.Application.Common.Interfaces;
using MassTransit;

namespace EventService.Infrastructure.Messaging;

public sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

    public Task PublishEventCreatedAsync(EventCreatedMessage message, CancellationToken cancellationToken = default)
        => _publishEndpoint.Publish(message, cancellationToken);
}
