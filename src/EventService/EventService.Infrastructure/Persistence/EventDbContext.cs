using EventService.Application.Common.Interfaces;
using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Persistence;

public sealed class EventDbContext : DbContext, IEventDbContext
{
    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
    }
}
