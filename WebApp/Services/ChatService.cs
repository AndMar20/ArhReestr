using DataLayer;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services;

public class ChatService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;
    private readonly TimeProvider _timeProvider;

    public ChatService(IDbContextFactory<ArhReestrContext> contextFactory, TimeProvider timeProvider)
    {
        _contextFactory = contextFactory;
        _timeProvider = timeProvider;
    }

    public async Task<int> SendAsync(int realEstateId, int senderId, int recipientId, string message, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var entity = new ChatMessage
        {
            RealEstateId = realEstateId,
            SenderId = senderId,
            RecipientId = recipientId,
            Message = message,
            SentAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        context.ChatMessages.Add(entity);
        await context.SaveChangesAsync(token);
        return entity.Id;
    }
}
