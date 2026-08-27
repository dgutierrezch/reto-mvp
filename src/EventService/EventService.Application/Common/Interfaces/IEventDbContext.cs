using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventService.Application.Common.Interfaces;

/// <summary>
/// Abstracción del DbContext para que Application no dependa de EF Core directamente
/// (regla de Clean Architecture: Application no conoce Infrastructure).
/// </summary>
public interface IEventDbContext
{
    DbSet<Event> Events { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
