using EventService.Application.Common.DTOs;
using MediatR;

namespace EventService.Application.Events.Queries.GetEvents;

public sealed record GetEventsQuery : IRequest<List<EventDto>>;
