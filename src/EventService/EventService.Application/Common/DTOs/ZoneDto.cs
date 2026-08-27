namespace EventService.Application.Common.DTOs;

public sealed record ZoneDto(Guid Id, string Name, decimal Price, int Capacity);

public sealed record CreateZoneDto(string Name, decimal Price, int Capacity);
