using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Api.Controllers;

/// <summary>
/// Endpoint solo de lectura para verificar en la demo qué mensajes se
/// procesaron, cuáles fallaron, y confirmar visualmente la idempotencia.
/// </summary>
[ApiController]
[Route("notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _db;

    public NotificationsController(NotificationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var logs = await _db.NotificationLogs
            .OrderByDescending(n => n.ProcessedAt)
            .Select(n => new
            {
                n.MessageId,
                n.EventId,
                n.EventName,
                n.Status,
                n.ProcessedAt,
                n.ErrorMessage
            })
            .ToListAsync(ct);

        return Ok(logs);
    }
}
