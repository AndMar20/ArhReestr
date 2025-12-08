using DataLayer;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using WebApp.Infrastructure;

namespace WebApp.Services;

/// <summary>
/// Возвращает справочные данные (агенты, роли) для форм и фильтров UI.
/// </summary>
public class LookupService
{
    private readonly ArhReestrContext _context;
    private readonly ILogger<LookupService> _logger;

    /// <summary>
    /// Внедряем контекст БД и логгер для работы со справочниками.
    /// </summary>
    public LookupService(ArhReestrContext context, ILogger<LookupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Загружает пользователей с ролью "agent" и собирает словарь Id -> ФИО.
    /// </summary>
    public async Task<Dictionary<int, string>> GetAgentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var agents = await _context.Users
                .AsNoTracking()
                .Where(u => u.Role != null && u.Role.Name == "agent")
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ThenBy(u => u.MiddleName)
                .Select(u => new { u.Id, u.LastName, u.FirstName, u.MiddleName })
                .ToListAsync(cancellationToken);

            var mapped = agents.ToDictionary(a => a.Id, a => FullNameFormatter.Combine(a.LastName, a.FirstName, a.MiddleName));
            return mapped;
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось загрузить список агентов");
            return new Dictionary<int, string>();
        }
    }

    /// <summary>
    /// Возвращает список ролей для настройки доступа в интерфейсе.
    /// </summary>
    public async Task<IReadOnlyList<DataLayer.Models.Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _context.Roles.AsNoTracking().OrderBy(r => r.Id).ToListAsync(cancellationToken);
            return roles;
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось загрузить роли пользователей");
            return Array.Empty<DataLayer.Models.Role>();
        }
    }
    /// <summary>
    /// Возвращает роль по системному имени. Бросает InvalidOperationException с понятным текстом при ошибке подключения.
    /// </summary>
    public async Task<DataLayer.Models.Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
            return role;
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось найти роль {RoleName}", roleName);
            return null;
        }
    }
}
