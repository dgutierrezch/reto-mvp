using EventService.Application.Common.DTOs;
using EventService.Application.Events.Commands.CreateEvent;
using EventService.Application.Events.Queries.GetEventById;
using EventService.Application.Events.Queries.GetEvents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Api.Controllers;

public sealed record CreateEventRequest(string Name, DateTime Date, string Location, List<CreateZoneDto> Zones);

[ApiController]
[Route("events")]
public sealed class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Crea un evento + zonas en una transacción y publica EventCreated de forma asíncrona.
    /// Solo Admin puede crear eventos (requisito de autorización por roles).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateEventRequest request, CancellationToken ct)
    {
        var command = new CreateEventCommand(request.Name, request.Date, request.Location, request.Zones);
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>
    /// Lista eventos con cache-aside en Redis (TTL 60s).
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<EventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EventDto>>> GetAll(CancellationToken ct)
    {
        var events = await _mediator.Send(new GetEventsQuery(), ct);
        return Ok(events);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken ct)
    {
        var evt = await _mediator.Send(new GetEventByIdQuery(id), ct);
        return evt is null ? NotFound() : Ok(evt);
    }
}
