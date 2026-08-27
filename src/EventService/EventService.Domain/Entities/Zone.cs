using EventService.Domain.Exceptions;

namespace EventService.Domain.Entities;

/// <summary>
/// Zona de un evento: representa un sector con precio y aforo propio
/// (ej. "General", "VIP", "Platea").
/// </summary>
public sealed class Zone
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Name { get; private set; } = default!;
    public decimal Price { get; private set; }
    public int Capacity { get; private set; }

    private Zone() { } // EF Core

    public Zone(string name, decimal price, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la zona es obligatorio.");
        if (price < 0)
            throw new DomainException("El precio de la zona no puede ser negativo.");
        if (capacity <= 0)
            throw new DomainException("La capacidad de la zona debe ser mayor a cero.");

        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Capacity = capacity;
    }

    internal void AssignToEvent(Guid eventId) => EventId = eventId;
}
