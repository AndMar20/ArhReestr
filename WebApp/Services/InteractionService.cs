using DataLayer;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using WebApp.Infrastructure;
using WebApp.ViewModels;
using System;

namespace WebApp.Services;

/// <summary>
/// Р Р°Р±РѕС‚Р°РµС‚ СЃ РѕР±СЂР°С‰РµРЅРёСЏРјРё РєР»РёРµРЅС‚РѕРІ: Р·Р°РіСЂСѓР·РєР°, СЃРѕР·РґР°РЅРёРµ Рё СЃРјРµРЅР° СЃС‚Р°С‚СѓСЃРѕРІ.
/// </summary>
public class InteractionService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;
    private readonly ILogger<InteractionService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly NotificationService _notificationService;
    private readonly AuditLogService _auditLogService;

    /// <summary>
    /// РџРѕР»СѓС‡Р°РµРј Р·Р°РІРёСЃРёРјРѕСЃС‚Рё EF Core, Р»РѕРіРёСЂРѕРІР°РЅРёСЏ Рё РїРѕСЃС‚Р°РІС‰РёРєР° РІСЂРµРјРµРЅРё (РґР»СЏ audit-РїРѕР»РµР№).
    /// </summary>
    public InteractionService(
        IDbContextFactory<ArhReestrContext> contextFactory,
        ILogger<InteractionService> logger,
        TimeProvider timeProvider,
        NotificationService notificationService,
        AuditLogService auditLogService)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _timeProvider = timeProvider;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// РћС‚РґР°С‘С‚ РѕР±СЂР°С‰РµРЅРёСЏ РєРѕРЅРєСЂРµС‚РЅРѕРіРѕ Р°РіРµРЅС‚Р°, РІРєР»СЋС‡Р°СЏ СЃРІСЏР·Р°РЅРЅС‹Рµ СЃСѓС‰РЅРѕСЃС‚Рё РґР»СЏ РІС‹РІРѕРґР° РІ UI.
    /// </summary>
    public async Task<IReadOnlyList<InteractionSummary>> GetAgentInteractionsAsync(int agentId, CancellationToken cancellationToken = default)
    {
        if (agentId <= 0)
        {
            throw new InvalidOperationException("РќРµ СѓРґР°Р»РѕСЃСЊ РѕРїСЂРµРґРµР»РёС‚СЊ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ-Р°РіРµРЅС‚Р°");
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var interactions = await context.Interactions
                .AsNoTracking()
                .Include(i => i.Agent)
                .Include(i => i.Client)
                .Include(i => i.RealEstate)
                .ThenInclude(r => r.House)
                .ThenInclude(h => h!.District)
                .Include(i => i.RealEstate)
                .ThenInclude(r => r.House)
                .ThenInclude(h => h!.Street)
                .Include(i => i.Status)
                .Where(i => i.AgentId == agentId && i.DeletedAt == null)
                .OrderByDescending(i => i.UpdatedAt)
                .Take(200)
                .ToListAsync(cancellationToken);

            return interactions.Select(Map).ToList();
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ РїРѕР»СѓС‡РёС‚СЊ РѕР±СЂР°С‰РµРЅРёСЏ РґР»СЏ Р°РіРµРЅС‚Р°");
            return Array.Empty<InteractionSummary>();
        }
    }

    /// <summary>
    /// РћС‚РґР°С‘С‚ РѕР±СЂР°С‰РµРЅРёСЏ РґР»СЏ Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂР°: РІС‹Р±РѕСЂРєР° РїРѕСЃР»РµРґРЅРёС… Р·Р°РїРёСЃРµР№ Р±РµР· С„РёР»СЊС‚СЂР° РїРѕ Р°РіРµРЅС‚Сѓ.
    /// </summary>
    public async Task<IReadOnlyList<InteractionSummary>> GetAdminInteractionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var interactions = await context.Interactions
                .AsNoTracking()
                .Include(i => i.Agent)
                .Include(i => i.Client)
                .Include(i => i.RealEstate)
                .ThenInclude(r => r.House)
                .ThenInclude(h => h!.District)
                .Include(i => i.RealEstate)
                .ThenInclude(r => r.House)
                .ThenInclude(h => h!.Street)
                .Include(i => i.Status)
                .Where(i => i.DeletedAt == null)
                .OrderByDescending(i => i.UpdatedAt)
                .Take(500)
                .ToListAsync(cancellationToken);

            return interactions.Select(Map).ToList();
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ РїРѕР»СѓС‡РёС‚СЊ РѕР±СЂР°С‰РµРЅРёСЏ РґР»СЏ Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂР°");
            return Array.Empty<InteractionSummary>();
        }
    }

    /// <summary>
    /// Р’РѕР·РІСЂР°С‰Р°РµС‚ СЃРїСЂР°РІРѕС‡РЅРёРє СЃС‚Р°С‚СѓСЃРѕРІ РґР»СЏ РІС‹РїР°РґР°СЋС‰РёС… СЃРїРёСЃРєРѕРІ.
    /// </summary>
    public async Task<IReadOnlyList<DataLayer.Models.InteractionStatus>> GetStatusesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var statuses = await context.InteractionStatuses.AsNoTracking().OrderBy(s => s.Id).ToListAsync(cancellationToken);
            return statuses;
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ Р·Р°РіСЂСѓР·РёС‚СЊ СЃС‚Р°С‚СѓСЃС‹ РѕР±СЂР°С‰РµРЅРёР№");
            return Array.Empty<DataLayer.Models.InteractionStatus>();
        }
    }

    /// <summary>
    /// РћР±РЅРѕРІР»СЏРµС‚ СЃС‚Р°С‚СѓСЃ РѕР±СЂР°С‰РµРЅРёСЏ, РїСЂРѕРІРµСЂСЏСЏ РїСЂР°РІР°: Р°РіРµРЅС‚ РјРѕР¶РµС‚ РјРµРЅСЏС‚СЊ С‚РѕР»СЊРєРѕ СЃРІРѕРё Р·Р°РїРёСЃРё.
    /// </summary>
    public async Task UpdateStatusAsync(InteractionUpdateRequest request, int userId, bool canUpdateAny, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var interaction = await context.Interactions
                .Include(i => i.RealEstate)
                .FirstOrDefaultAsync(i => i.Id == request.InteractionId, cancellationToken);
            if (interaction is null)
            {
                throw new InvalidOperationException("Р’Р·Р°РёРјРѕРґРµР№СЃС‚РІРёРµ РЅРµ РЅР°Р№РґРµРЅРѕ");
            }

            if (!canUpdateAny && interaction.AgentId != userId)
            {
                throw new InvalidOperationException("РќРµС‚ РїСЂР°РІ РЅР° РёР·РјРµРЅРµРЅРёРµ Р·Р°РїРёСЃРё");
            }

            var oldStatusId = interaction.StatusId;
            interaction.StatusId = request.StatusId;
            interaction.Notes = request.Notes;
            interaction.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
            await ApplyObjectStatusFromInteractionAsync(context, interaction, request.StatusId, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            await NotifyInteractionChangedAsync(interaction, userId, oldStatusId, request.StatusId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ РѕР±РЅРѕРІРёС‚СЊ РІР·Р°РёРјРѕРґРµР№СЃС‚РІРёРµ {Id}", request.InteractionId);
            throw;
        }
    }

    /// <summary>
    /// РџРѕР»РЅРѕРµ РѕР±РЅРѕРІР»РµРЅРёРµ РѕР±СЂР°С‰РµРЅРёСЏ СЃРѕ СЃС‚РѕСЂРѕРЅС‹ Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂР°: СЃРјРµРЅР° СЃС‚Р°С‚СѓСЃР°, Р°РіРµРЅС‚Р° Рё РєРѕРјРјРµРЅС‚Р°СЂРёСЏ.
    /// </summary>
    public async Task UpdateByAdminAsync(AdminInteractionUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.AgentId <= 0)
        {
            throw new InvalidOperationException("РќСѓР¶РЅРѕ РІС‹Р±СЂР°С‚СЊ СЂРёРµР»С‚РѕСЂР° РґР»СЏ РѕР±СЂР°С‰РµРЅРёСЏ");
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var interaction = await context.Interactions
                .Include(i => i.RealEstate)
                .FirstOrDefaultAsync(i => i.Id == request.InteractionId, cancellationToken);
            if (interaction is null)
            {
                throw new InvalidOperationException("Р’Р·Р°РёРјРѕРґРµР№СЃС‚РІРёРµ РЅРµ РЅР°Р№РґРµРЅРѕ");
            }

            var oldStatusId = interaction.StatusId;
            var oldAgentId = interaction.AgentId;
            interaction.StatusId = request.StatusId;
            interaction.AgentId = request.AgentId;
            interaction.Notes = request.Notes;
            interaction.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
            await ApplyObjectStatusFromInteractionAsync(context, interaction, request.StatusId, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            await NotifyInteractionChangedAsync(interaction, null, oldStatusId, request.StatusId, cancellationToken);
            if (oldAgentId != request.AgentId)
            {
                await _notificationService.CreateAsync(request.AgentId, "РќР°Р·РЅР°С‡РµРЅР° Р·Р°СЏРІРєР°", $"Р’Р°Рј РЅР°Р·РЅР°С‡РµРЅР° Р·Р°СЏРІРєР° #{interaction.Id} РїРѕ РѕР±СЉРµРєС‚Сѓ #{interaction.RealEstateId}.", $"/chat?realEstateId={interaction.RealEstateId}&peerId={interaction.ClientId}", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ РѕР±РЅРѕРІРёС‚СЊ РІР·Р°РёРјРѕРґРµР№СЃС‚РІРёРµ {Id} Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂРѕРј", request.InteractionId);
            throw;
        }
    }

    /// <summary>
    /// РЎРѕР·РґР°С‘С‚ РЅРѕРІРѕРµ РѕР±СЂР°С‰РµРЅРёРµ Рё РІС‹СЃС‚Р°РІР»СЏРµС‚ РїРµСЂРІСѓСЋ СЃС‚Р°РґРёСЋ СЃС‚Р°С‚СѓСЃР°.
    /// </summary>
    public async Task<int> CreateInteractionAsync(int clientId, int agentId, int realEstateId, string? notes, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var realEstate = await context.RealEstates
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.Id == realEstateId && r.DeletedAt == null, cancellationToken);
            if (realEstate is null || realEstate.Status?.Code != "active")
            {
                throw new InvalidOperationException("Заявку нельзя оставить: объект недоступен.");
            }

            var existing = await context.Interactions
                .Where(i => i.ClientId == clientId && i.RealEstateId == realEstateId)
                .OrderByDescending(i => i.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing is not null && existing.DeletedAt == null)
            {
                throw new InvalidOperationException("Заявка по этому объекту уже создана.");
            }

            var statusId = await context.InteractionStatuses
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .FirstAsync(cancellationToken);

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            DataLayer.Models.Interaction entity;
            var auditAction = "create";

            if (existing is not null)
            {
                existing.AgentId = agentId;
                existing.StatusId = statusId;
                existing.DeletedAt = null;
                existing.ContactedAt = now;
                existing.UpdatedAt = now;
                existing.Notes = notes;
                entity = existing;
                auditAction = "restore";
            }
            else
            {
                entity = new DataLayer.Models.Interaction
                {
                    AgentId = agentId,
                    ClientId = clientId,
                    RealEstateId = realEstateId,
                    StatusId = statusId,
                    ContactedAt = now,
                    UpdatedAt = now,
                    Notes = notes
                };

                context.Interactions.Add(entity);
            }

            await context.SaveChangesAsync(cancellationToken);
            await _notificationService.CreateAsync(agentId, "Новая заявка", $"Клиент оставил заявку по объекту #{realEstateId}.", $"/chat?realEstateId={realEstateId}&peerId={clientId}", cancellationToken);
            await _notificationService.CreateAsync(clientId, "Заявка создана", $"Ваша заявка по объекту #{realEstateId} отправлена риелтору.", $"/chat?realEstateId={realEstateId}&peerId={agentId}", cancellationToken);
            await _auditLogService.WriteAsync("Interaction", auditAction, entity.Id, clientId, null, $"Создана заявка по объекту #{realEstateId}", cancellationToken);
            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось создать взаимодействие");
            throw;
        }
    }

    public async Task<int?> GetActiveInteractionIdAsync(int clientId, int realEstateId, CancellationToken cancellationToken = default)
    {
        if (clientId <= 0 || realEstateId <= 0)
        {
            return null;
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Interactions
            .AsNoTracking()
            .Where(i => i.ClientId == clientId && i.RealEstateId == realEstateId && i.DeletedAt == null)
            .Select(i => (int?)i.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task ApplyObjectStatusFromInteractionAsync(
        ArhReestrContext context,
        DataLayer.Models.Interaction interaction,
        int statusId,
        CancellationToken cancellationToken)
    {
        if (interaction.RealEstate is null)
        {
            return;
        }

        var statusName = await context.InteractionStatuses
            .Where(s => s.Id == statusId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync(cancellationToken);

        string? targetCode = statusName switch
        {
            var name when name?.Contains("Р·Р°РІРµСЂС€", StringComparison.OrdinalIgnoreCase) == true => "sold",
            var name when name?.Contains("СЂР°Р±РѕС‚", StringComparison.OrdinalIgnoreCase) == true => "reserved",
            _ => null
        };

        if (targetCode is null)
        {
            return;
        }

        var objectStatusId = await context.RealEstateStatuses
            .Where(s => s.Code == targetCode)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (objectStatusId is not null)
        {
            interaction.RealEstate.StatusId = objectStatusId.Value;
        }

        if (targetCode == "sold")
        {
            var dealExists = await context.Deals.AnyAsync(d => d.InteractionId == interaction.Id, cancellationToken);
            if (!dealExists)
            {
                var amount = interaction.RealEstate.Price;
                var now = _timeProvider.GetUtcNow().UtcDateTime;
                context.Deals.Add(new DataLayer.Models.Deal
                {
                    InteractionId = interaction.Id,
                    RealEstateId = interaction.RealEstateId,
                    AgentId = interaction.AgentId,
                    ClientId = interaction.ClientId,
                    Amount = amount,
                    Commission = decimal.Round(amount * 0.03m, 2),
                    ClosedAt = now,
                    CreatedAt = now
                });
            }
        }
    }

    private async Task NotifyInteractionChangedAsync(
        DataLayer.Models.Interaction interaction,
        int? actorUserId,
        int oldStatusId,
        int newStatusId,
        CancellationToken cancellationToken)
    {
        await _auditLogService.WriteAsync(
            "Interaction",
            "status-change",
            interaction.Id,
            actorUserId,
            oldStatusId.ToString(),
            newStatusId.ToString(),
            cancellationToken);

        if (oldStatusId != newStatusId)
        {
            await _notificationService.CreateAsync(interaction.ClientId, "РЎС‚Р°С‚СѓСЃ Р·Р°СЏРІРєРё РёР·РјРµРЅС‘РЅ", $"Р—Р°СЏРІРєР° #{interaction.Id} РѕР±РЅРѕРІР»РµРЅР°.", $"/chat?realEstateId={interaction.RealEstateId}&peerId={interaction.AgentId}", cancellationToken);
            await _notificationService.CreateAsync(interaction.AgentId, "РЎС‚Р°С‚СѓСЃ Р·Р°СЏРІРєРё РёР·РјРµРЅС‘РЅ", $"Р—Р°СЏРІРєР° #{interaction.Id} РѕР±РЅРѕРІР»РµРЅР°.", $"/chat?realEstateId={interaction.RealEstateId}&peerId={interaction.ClientId}", cancellationToken);
        }
    }

    public async Task<InteractionSummary?> GetDialogInteractionAsync(
        int userId,
        int realEstateId,
        int peerId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || realEstateId <= 0 || peerId <= 0)
        {
            return null;
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var interaction = await context.Interactions
            .AsNoTracking()
            .Include(i => i.Agent)
            .Include(i => i.Client)
            .Include(i => i.RealEstate)
                .ThenInclude(r => r.House)
                .ThenInclude(h => h!.District)
            .Include(i => i.RealEstate)
                .ThenInclude(r => r.House)
                .ThenInclude(h => h!.Street)
            .Include(i => i.Status)
            .Where(i => i.RealEstateId == realEstateId && i.DeletedAt == null)
            .Where(i => isAdmin ||
                (i.ClientId == userId && i.AgentId == peerId) ||
                (i.ClientId == peerId && i.AgentId == userId))
            .OrderByDescending(i => i.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return interaction is null ? null : Map(interaction);
    }

    public async Task CancelByClientAndEstateAsync(int clientId, int realEstateId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var interaction = await context.Interactions
            .FirstOrDefaultAsync(i => i.ClientId == clientId && i.RealEstateId == realEstateId && i.DeletedAt == null, cancellationToken);
        if (interaction is null)
        {
            throw new InvalidOperationException("Р—Р°СЏРІРєР° РЅРµ РЅР°Р№РґРµРЅР°.");
        }

        interaction.DeletedAt = _timeProvider.GetUtcNow().UtcDateTime;
        interaction.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelByClientAsync(int interactionId, int clientId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var interaction = await context.Interactions
            .FirstOrDefaultAsync(i => i.Id == interactionId && i.ClientId == clientId && i.DeletedAt == null, cancellationToken);
        if (interaction is null)
        {
            throw new InvalidOperationException("Р—Р°СЏРІРєР° РЅРµ РЅР°Р№РґРµРЅР°.");
        }

        interaction.DeletedAt = _timeProvider.GetUtcNow().UtcDateTime;
        interaction.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
        interaction.Notes = string.IsNullOrWhiteSpace(interaction.Notes)
            ? "Р—Р°СЏРІРєР° РѕС‚РјРµРЅРµРЅР° РєР»РёРµРЅС‚РѕРј."
            : $"{interaction.Notes} | Р—Р°СЏРІРєР° РѕС‚РјРµРЅРµРЅР° РєР»РёРµРЅС‚РѕРј.";
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// РњР°РїРїРёРЅРі СЃСѓС‰РЅРѕСЃС‚Рё EF Core РІ РјРѕРґРµР»СЊ РґР»СЏ РѕС‚РѕР±СЂР°Р¶РµРЅРёСЏ РЅР° СЌРєСЂР°РЅРµ.
    /// </summary>
    private static InteractionSummary Map(DataLayer.Models.Interaction interaction)
    {
        return new InteractionSummary(
            interaction.Id,
            interaction.ClientId,
            interaction.Client?.GetFullName() ?? "",
            interaction.Agent?.GetFullName() ?? "",
            interaction.AgentId,
            interaction.RealEstateId,
            AddressFormatter.Format(interaction.RealEstate?.House),
            interaction.StatusId,
            interaction.Status?.Name ?? string.Empty,
            interaction.ContactedAt,
            interaction.UpdatedAt,
            interaction.Notes
        )
        {
            ClientPhone = interaction.Client?.Phone ?? string.Empty,
            AgentPhone = interaction.Agent?.Phone ?? string.Empty
        };
    }

    /// <summary>
    /// РћС‚РґР°С‘С‚ РѕР±СЂР°С‰РµРЅРёСЏ РєРѕРЅРєСЂРµС‚РЅРѕРіРѕ РєР»РёРµРЅС‚Р°, РІРєР»СЋС‡Р°СЏ СЃРІСЏР·Р°РЅРЅС‹Рµ СЃСѓС‰РЅРѕСЃС‚Рё РґР»СЏ РІС‹РІРѕРґР° РІ UI.
    /// </summary>
    public async Task<IReadOnlyList<InteractionSummary>> GetClientInteractionsAsync(
    int clientId,
    CancellationToken cancellationToken = default)
    {
        if (clientId <= 0)
            throw new InvalidOperationException("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Р№ РїРѕР»СЊР·РѕРІР°С‚РµР»СЊ.");

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var interactions = await context.Interactions
                .AsNoTracking()
                .Include(i => i.Agent)
                .Include(i => i.Client)
                .Include(i => i.RealEstate)
                    .ThenInclude(r => r.House)
                    .ThenInclude(h => h!.Street)
                .Include(i => i.Status)
                .Where(i => i.ClientId == clientId && i.DeletedAt == null)
                .OrderByDescending(i => i.UpdatedAt)
                .ToListAsync(cancellationToken);

            return interactions.Select(Map).ToList();
        }
        catch
        {
            return Array.Empty<InteractionSummary>();
        }
    }
}

