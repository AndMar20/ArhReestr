using DataLayer;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using WebApp.ViewModels;

namespace WebApp.Services;

public class ChatService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;
    private readonly TimeProvider _timeProvider;
    private readonly NotificationService _notificationService;

    public ChatService(
        IDbContextFactory<ArhReestrContext> contextFactory,
        TimeProvider timeProvider,
        NotificationService notificationService)
    {
        _contextFactory = contextFactory;
        _timeProvider = timeProvider;
        _notificationService = notificationService;
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

        var senderName = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == senderId)
            .Select(user => user.LastName + " " + user.FirstName)
            .FirstOrDefaultAsync(token);

        var linkUrl = $"/chat?realEstateId={realEstateId}&peerId={senderId}";
        await _notificationService.CreateAsync(
            recipientId,
            "Новое сообщение",
            $"{(string.IsNullOrWhiteSpace(senderName) ? "Собеседник" : senderName)} написал по объекту #{realEstateId}.",
            linkUrl,
            token);
        return entity.Id;
    }

    public async Task<IReadOnlyList<ChatDialogItem>> GetDialogsAsync(int userId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);

        var messages = await context.ChatMessages
            .AsNoTracking()
            .Where(m => m.SenderId == userId || m.RecipientId == userId)
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Include(m => m.RealEstate)
            .ThenInclude(r => r.House)
            .ThenInclude(h => h!.Street)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync(token);

        var dialogs = messages
            .GroupBy(m => new
            {
                m.RealEstateId,
                PeerId = m.SenderId == userId ? m.RecipientId : m.SenderId
            })
            .Select(g =>
            {
                var last = g.OrderByDescending(x => x.SentAt).First();
                var peer = last.SenderId == userId ? last.Recipient : last.Sender;
                var hasUnread = g.Any(x => x.RecipientId == userId && x.ReadAt == null);
                var address = AddressFormatter.Format(last.RealEstate?.House);
                return new ChatDialogItem(g.Key.RealEstateId, g.Key.PeerId, peer?.GetFullName() ?? "Пользователь", address, last.Message, last.SentAt, hasUnread);
            })
            .OrderByDescending(d => d.LastMessageAt)
            .ToList();

        return dialogs;
    }

    public async Task<IReadOnlyList<ChatMessageItem>> GetMessagesAsync(int userId, int realEstateId, int peerId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var messages = await context.ChatMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.RealEstateId == realEstateId &&
                       ((m.SenderId == userId && m.RecipientId == peerId) ||
                        (m.SenderId == peerId && m.RecipientId == userId)))
            .OrderBy(m => m.SentAt)
            .ToListAsync(token);

        return messages.Select(m => new ChatMessageItem(m.Id, m.SenderId, m.Sender?.GetFullName() ?? "Пользователь", m.Message, m.SentAt, m.ReadAt)).ToList();
    }

    public async Task MarkAsReadAsync(int userId, int realEstateId, int peerId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var unread = await context.ChatMessages
            .Where(m => m.RealEstateId == realEstateId && m.SenderId == peerId && m.RecipientId == userId && m.ReadAt == null)
            .ToListAsync(token);

        if (unread.Count == 0) return;
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        foreach (var item in unread)
        {
            item.ReadAt = now;
        }

        await context.SaveChangesAsync(token);
    }
}
