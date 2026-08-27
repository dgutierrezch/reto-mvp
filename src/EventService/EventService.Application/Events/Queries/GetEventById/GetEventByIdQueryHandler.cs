using EventService.Application.Common.DTOs;
using EventService.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventService.Application.Events.Queries.GetEventById;

public sealed class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto?>
{
    private readonly IEventDbContext _db;

    public GetEventByIdQueryHandler(IEventDbContext db) => _db = db;

    public async Task<EventDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.Events
            .AsNoTracking()
            .Include(e => e.Zones)
            .Where(e => e.Id == request.Id)
            .Select(e => new EventDto(
                e.Id,
                e.Name,
                e.Date,
                e.Location,
                e.Status.ToString(),
                e.CreatedAt,
                e.Zones.Select(z => new ZoneDto(z.Id, z.Name, z.Price, z.Capacity)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
