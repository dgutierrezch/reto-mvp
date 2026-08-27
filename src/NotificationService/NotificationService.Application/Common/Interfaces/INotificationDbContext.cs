using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Common.Interfaces;

public interface INotificationDbContext
{
    DbSet<NotificationLog> NotificationLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
