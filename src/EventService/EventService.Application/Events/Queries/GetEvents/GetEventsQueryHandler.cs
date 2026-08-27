using EventService.Application.Common.DTOs;
using EventService.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventService.Application.Events.Queries.GetEvents;

/// <summary>
/// Cache-aside: intenta leer de Redis primero; si no está, consulta la DB
/// y rellena el cache con un TTL corto. Se invalida al crear/publicar un evento.
/// </summary>
public sealed class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, List<EventDto>>
{
    private const string CacheKey = "events:all";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private readonly IEventDbContext _db;
    private readonly ICacheService _cache;

    public GetEventsQueryHandler(IEventDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<List<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync<List<EventDto>>(CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var events = await _db.Events
            .AsNoTracking()
            .Include(e => e.Zones)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new EventDto(
                e.Id,
                e.Name,
                e.Date,
                e.Location,
                e.Status.ToString(),
                e.CreatedAt,
                e.Zones.Select(z => new ZoneDto(z.Id, z.Name, z.Price, z.Capacity)).ToList()))
            .ToListAsync(cancellationToken);

        await _cache.SetAsync(CacheKey, events, CacheTtl, cancellationToken);

        return events;
    }
}
