using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Common.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence;

public sealed class NotificationDbContext : DbContext, INotificationDbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
