using EventService.Application.Common.DTOs;
using MediatR;

namespace EventService.Application.Events.Commands.CreateEvent;

public sealed record CreateEventCommand(
    string Name,
    DateTime Date,
    string Location,
    List<CreateZoneDto> Zones) : IRequest<Guid>;
