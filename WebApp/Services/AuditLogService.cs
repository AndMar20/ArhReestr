using DataLayer;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services;

public class AuditLogService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;
    private readonly TimeProvider _timeProvider;

    public AuditLogService(IDbContextFactory<ArhReestrContext> contextFactory, TimeProvider timeProvider)
    {
        _contextFactory = contextFactory;
        _timeProvider = timeProvider;
    }

    public async Task WriteAsync(
        string entityType,
        string action,
        int? entityId = null,
        int? actorUserId = null,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(
        string? search = null,
        IReadOnlyCollection<string>? entityTypes = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.AuditLogs
            .AsNoTracking()
            .Include(log => log.ActorUser)
            .OrderByDescending(log => log.CreatedAt)
            .AsQueryable();

        if (entityTypes is { Count: > 0 })
        {
            query = query.Where(log => entityTypes.Contains(log.EntityType));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(log =>
                log.EntityType.Contains(term)
                || log.Action.Contains(term)
                || (log.ActorUser != null && (log.ActorUser.LastName.Contains(term) || log.ActorUser.FirstName.Contains(term))));
        }

        return await query.Take(200).ToListAsync(cancellationToken);
    }
}
