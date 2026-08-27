namespace EventService.Application.Common.DTOs;

public sealed record EventDto(
    Guid Id,
    string Name,
    DateTime Date,
    string Location,
    string Status,
    DateTime CreatedAt,
    List<ZoneDto> Zones);
