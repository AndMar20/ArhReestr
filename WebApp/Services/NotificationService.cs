using DataLayer;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services;

public class NotificationService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;
    private readonly TimeProvider _timeProvider;

    public NotificationService(IDbContextFactory<ArhReestrContext> contextFactory, TimeProvider timeProvider)
    {
        _contextFactory = contextFactory;
        _timeProvider = timeProvider;
    }

    public async Task CreateAsync(int userId, string title, string message, CancellationToken token = default)
    {
        await CreateAsync(userId, title, message, null, token);
    }

    public async Task CreateAsync(int userId, string title, string message, string? linkUrl, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            LinkUrl = linkUrl,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        });
        await context.SaveChangesAsync(token);
    }

    public async Task<IReadOnlyList<Notification>> GetForUserAsync(int userId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        return await context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync(token);
    }

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        return await context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead, token);
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var notifications = await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(token);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await context.SaveChangesAsync(token);
    }

    public async Task MarkAsReadAsync(int userId, int notificationId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, token);

        if (notification is null || notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        await context.SaveChangesAsync(token);
    }
}
