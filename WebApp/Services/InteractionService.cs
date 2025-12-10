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

    /// <summary>
    /// Получаем зависимости EF Core, логирования и поставщика времени (для audit-полей).
    /// </summary>
    public InteractionService(
        IDbContextFactory<ArhReestrContext> contextFactory,
        ILogger<InteractionService> logger,
        TimeProvider timeProvider)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _timeProvider = timeProvider;
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
                .ThenInclude(h => h.District)
                .Include(i => i.RealEstate)
                .ThenInclude(r => r.House)
                .ThenInclude(h => h.Street)
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
                .ThenInclude(h => h.District)
                .Include(i => i.RealEstate)
                .ThenInclude(r => r.House)
                .ThenInclude(h => h.Street)
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

            var interaction = await context.Interactions.FirstOrDefaultAsync(i => i.Id == request.InteractionId, cancellationToken);
            if (interaction is null)
            {
                throw new InvalidOperationException("Взаимодействие не найдено");
            }

            if (!canUpdateAny && interaction.AgentId != userId)
            {
                throw new InvalidOperationException("Нет прав на изменение записи");
            }

            interaction.StatusId = request.StatusId;
            interaction.Notes = request.Notes;
            interaction.UpdatedAt = _timeProvider.GetMoscowDateTime();

            await context.SaveChangesAsync(cancellationToken);
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

            var interaction = await context.Interactions.FirstOrDefaultAsync(i => i.Id == request.InteractionId, cancellationToken);
            if (interaction is null)
            {
                throw new InvalidOperationException("Взаимодействие не найдено");
            }

            interaction.StatusId = request.StatusId;
            interaction.AgentId = request.AgentId;
            interaction.Notes = request.Notes;
            interaction.UpdatedAt = _timeProvider.GetMoscowDateTime();

            await context.SaveChangesAsync(cancellationToken);
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

            var statusId = await context.InteractionStatuses
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .FirstAsync(cancellationToken);

            var entity = new DataLayer.Models.Interaction
            {
                AgentId = agentId,
                ClientId = clientId,
                RealEstateId = realEstateId,
                StatusId = statusId,
                ContactedAt = _timeProvider.GetMoscowDateTime(),
                UpdatedAt = _timeProvider.GetMoscowDateTime(),
                Notes = notes
            };

            context.Interactions.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось создать взаимодействие");
            throw;
        }
    }

    /// <summary>
    /// Маппинг сущности EF Core в модель для отображения на экране.
    /// </summary>
    private static InteractionSummary Map(DataLayer.Models.Interaction interaction)
    {
        return new InteractionSummary(
            interaction.Id,
            interaction.Client?.GetFullName() ?? "",
            interaction.Agent?.GetFullName() ?? "",
            interaction.AgentId,
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
                    .ThenInclude(h => h.Street)
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
