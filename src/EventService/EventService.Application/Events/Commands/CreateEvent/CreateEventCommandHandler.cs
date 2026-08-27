using EventPlatform.Contracts.Messages;
using EventService.Application.Common.Interfaces;
using EventService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventService.Application.Events.Commands.CreateEvent;

/// <summary>
/// Crea el evento + zonas en una única transacción (vía SaveChanges) y,
/// solo si la escritura fue exitosa, publica EventCreated de forma asíncrona.
/// </summary>
public sealed class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventDbContext _db;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<CreateEventCommandHandler> _logger;

    public CreateEventCommandHandler(
        IEventDbContext db,
        IEventPublisher publisher,
        ILogger<CreateEventCommandHandler> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var zones = request.Zones
            .Select(z => new Zone(z.Name, z.Price, z.Capacity))
            .ToList();

        var evt = Domain.Entities.Event.Create(request.Name, request.Date, request.Location, zones);

        _db.Events.Add(evt);
        await _db.SaveChangesAsync(cancellationToken);

        var message = new EventCreatedMessage
        {
            EventId = evt.Id,
            Name = evt.Name,
            OccurredAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        };

        try
        {
            await _publisher.PublishEventCreatedAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            // La escritura en DB ya se confirmó; un fallo al publicar no debe
            // tumbar la request del cliente. Se loguea para reconciliación/outbox futuro.
            _logger.LogError(ex, "No se pudo publicar EventCreated para el evento {EventId}", evt.Id);
        }

        return evt.Id;
    }
}
