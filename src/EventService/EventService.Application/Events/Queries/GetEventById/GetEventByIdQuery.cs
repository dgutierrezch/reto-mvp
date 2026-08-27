using EventService.Application.Common.DTOs;
using MediatR;

namespace EventService.Application.Events.Queries.GetEventById;

public sealed record GetEventByIdQuery(Guid Id) : IRequest<EventDto?>;
