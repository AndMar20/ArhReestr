using DataLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using WebApp.Identity;
using WebApp.Infrastructure;
using WebApp.ViewModels;

namespace WebApp.Services;

/// <summary>
/// Сервис для управления пользователями администратором: просмотр и смена ролей.
/// </summary>
public class AdminUserService
{
    private readonly ArhReestrContext _context;
    private readonly ILogger<AdminUserService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserService(
        ArhReestrContext context,
        ILogger<AdminUserService> logger,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    /// <summary>
    /// Возвращает пользователей с их ролями для таблицы администратора.
    /// </summary>
    public async Task<IReadOnlyList<UserListItem>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Здесь работаем с сущностью БД (DataLayer.Models.User),
            // у которой есть RoleId и навигационное свойство Role.
            var users = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(cancellationToken);

            return users
                .Select(u => new UserListItem
                {
                    Id = u.Id,
                    FullName = FullNameFormatter.Combine(u.LastName, u.FirstName, u.MiddleName),
                    Email = u.Email,
                    Phone = u.Phone,
                    RoleName = u.Role?.Name ?? string.Empty,
                    RoleDisplayName = u.Role?.DisplayName ?? u.Role?.Name ?? "—",
                    CreatedAt = u.CreatedAt
                })
                .ToList();
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось загрузить пользователей");
            return Array.Empty<UserListItem>();
        }
    }

    /// <summary>
    /// Обновляет роль пользователя. Исключения пробрасываются для отображения читаемых ошибок в UI.
    /// </summary>
    public async Task UpdateUserRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
    {
        // 1. Берём пользователя из Identity (ApplicationUser), а НЕ DataLayer.Models.User
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new InvalidOperationException("Пользователь не найден или удалён.");
        }

        // 2. Находим роль в нашей таблице ролей
        var targetRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);

        if (targetRole is null)
        {
            throw new InvalidOperationException("Указанная роль недоступна.");
        }

        // Если уже эта роль — ничего не делаем
        if (user.RoleId == targetRole.Id)
        {
            return;
        }

        // 3. Снимаем все текущие роли и ставим новую
        var currentRoles = await _userManager.GetRolesAsync(user);

        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                var message = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Не удалось убрать старые роли пользователя {UserId}: {Message}", userId, message);
                throw new InvalidOperationException(message);
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, targetRole.Name);
        if (!addResult.Succeeded)
        {
            var message = string.Join("; ", addResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Не удалось обновить роль пользователя {UserId}: {Message}", userId, message);
            throw new InvalidOperationException(message);
        }

        // 4. Обновляем привязку к роли в самой сущности
        user.RoleId = targetRole.Id;
        user.RoleName = targetRole.Name; // это свойство есть у ApplicationUser (ты его используешь в Register)

        await _userManager.UpdateAsync(user);
    }
}
