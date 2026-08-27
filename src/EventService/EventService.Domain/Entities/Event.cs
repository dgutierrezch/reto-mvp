using EventService.Domain.Enums;
using EventService.Domain.Exceptions;

namespace EventService.Domain.Entities;

/// <summary>
/// Aggregate root del dominio de catálogo. Un Event es dueño de sus Zones:
/// no existen zonas sin evento, y se crean/persisten juntas en una transacción.
/// </summary>
public sealed class Event
{
    private readonly List<Zone> _zones = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTime Date { get; private set; }
    public string Location { get; private set; } = default!;
    public EventStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Zone> Zones => _zones.AsReadOnly();

    private Event() { } // EF Core

    public static Event Create(string name, DateTime date, string location, IEnumerable<Zone> zones)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del evento es obligatorio.");
        if (string.IsNullOrWhiteSpace(location))
            throw new DomainException("El lugar del evento es obligatorio.");

        var zoneList = zones?.ToList() ?? new List<Zone>();
        if (zoneList.Count == 0)
            throw new DomainException("El evento debe tener al menos una zona.");

        var evt = new Event
        {
            Id = Guid.NewGuid(),
            Name = name,
            Date = date,
            Location = location,
            Status = EventStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var zone in zoneList)
        {
            zone.AssignToEvent(evt.Id);
            evt._zones.Add(zone);
        }

        return evt;
    }

    public void Publish()
    {
        if (Status != EventStatus.Draft)
            throw new DomainException("Solo un evento en estado Draft puede publicarse.");

        Status = EventStatus.Published;
    }
}
