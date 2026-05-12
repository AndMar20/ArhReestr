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
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        context.Notifications.Add(new Notification { UserId = userId, Title = title, Message = message, CreatedAt = _timeProvider.GetUtcNow().UtcDateTime });
        await context.SaveChangesAsync(token);
    }
}
