using DataLayer;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using WebApp.Infrastructure;
using WebApp.ViewModels;
using System;

namespace WebApp.Services;

/// <summary>
/// Работает с обращениями клиентов: загрузка, создание и смена статусов.
/// </summary>
public class InteractionService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;
    private readonly ILogger<InteractionService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly NotificationService _notificationService;
    private readonly AuditLogService _auditLogService;

    /// <summary>
    /// Получаем зависимости EF Core, логирования и поставщика времени (для audit-полей).
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
    /// Отдаёт обращения конкретного агента, включая связанные сущности для вывода в UI.
    /// </summary>
    public async Task<IReadOnlyList<InteractionSummary>> GetAgentInteractionsAsync(int agentId, CancellationToken cancellationToken = default)
    {
        if (agentId <= 0)
        {
            throw new InvalidOperationException("Не удалось определить пользователя-агента");
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
            _logger.LogError(ex, "Не удалось получить обращения для агента");
            return Array.Empty<InteractionSummary>();
        }
    }

    /// <summary>
    /// Отдаёт обращения для администратора: выборка последних записей без фильтра по агенту.
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
            _logger.LogError(ex, "Не удалось получить обращения для администратора");
            return Array.Empty<InteractionSummary>();
        }
    }

    /// <summary>
    /// Возвращает справочник статусов для выпадающих списков.
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
            _logger.LogError(ex, "Не удалось загрузить статусы обращений");
            return Array.Empty<DataLayer.Models.InteractionStatus>();
        }
    }

    /// <summary>
    /// Обновляет статус обращения, проверяя права: агент может менять только свои записи.
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
                throw new InvalidOperationException("Взаимодействие не найдено");
            }

            if (!canUpdateAny && interaction.AgentId != userId)
            {
                throw new InvalidOperationException("Нет прав на изменение записи");
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
            _logger.LogError(ex, "Не удалось обновить взаимодействие {Id}", request.InteractionId);
            throw;
        }
    }

    /// <summary>
    /// Полное обновление обращения со стороны администратора: смена статуса, агента и комментария.
    /// </summary>
    public async Task UpdateByAdminAsync(AdminInteractionUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (request.AgentId <= 0)
        {
            throw new InvalidOperationException("Нужно выбрать риелтора для обращения");
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var interaction = await context.Interactions
                .Include(i => i.RealEstate)
                .FirstOrDefaultAsync(i => i.Id == request.InteractionId, cancellationToken);
            if (interaction is null)
            {
                throw new InvalidOperationException("Взаимодействие не найдено");
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
                await _notificationService.CreateAsync(request.AgentId, "Назначена заявка", $"Вам назначена заявка #{interaction.Id} по объекту #{interaction.RealEstateId}.", $"/chat?realEstateId={interaction.RealEstateId}&peerId={interaction.ClientId}", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось обновить взаимодействие {Id} администратором", request.InteractionId);
            throw;
        }
    }

    /// <summary>
    /// Создаёт новое обращение и выставляет первую стадию статуса.
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
                throw new InvalidOperationException("Заявку нельзя создать: объект недоступен.");
            }

            var existing = await context.Interactions
                .Where(i => i.ClientId == clientId && i.RealEstateId == realEstateId)
                .OrderByDescending(i => i.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing is not null && existing.DeletedAt == null)
            {
                throw new InvalidOperationException("Заявка уже существует или недоступна.");
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
            var name when name?.Contains("заверш", StringComparison.OrdinalIgnoreCase) == true => "sold",
            var name when name?.Contains("работ", StringComparison.OrdinalIgnoreCase) == true => "reserved",
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
            await _notificationService.CreateAsync(interaction.ClientId, "Статус заявки изменён", $"Заявка #{interaction.Id} обновлена.", $"/chat?realEstateId={interaction.RealEstateId}&peerId={interaction.AgentId}", cancellationToken);
            await _notificationService.CreateAsync(interaction.AgentId, "Статус заявки изменён", $"Заявка #{interaction.Id} обновлена.", $"/chat?realEstateId={interaction.RealEstateId}&peerId={interaction.ClientId}", cancellationToken);
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
            throw new InvalidOperationException("Заявка не найдена.");
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
            throw new InvalidOperationException("Заявка не найдена.");
        }

        interaction.DeletedAt = _timeProvider.GetUtcNow().UtcDateTime;
        interaction.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
        interaction.Notes = string.IsNullOrWhiteSpace(interaction.Notes)
            ? "Заявка отменена клиентом."
            : $"{interaction.Notes} | Заявка отменена клиентом.";
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Маппинг сущности EF Core в модель для отображения на экране.
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
    /// Отдаёт обращения конкретного клиента, включая связанные сущности для вывода в UI.
    /// </summary>
    public async Task<IReadOnlyList<InteractionSummary>> GetClientInteractionsAsync(
    int clientId,
    CancellationToken cancellationToken = default)
    {
        if (clientId <= 0)
            throw new InvalidOperationException("Некорректный пользователь.");

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

