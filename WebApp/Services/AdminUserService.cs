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
            var users = await _context.Users
                .Where(u => u.DeletedAt == null)
                .Include(u => u.Role)
                .AsNoTracking()
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(cancellationToken);

            return users.Select(MapToListItem).ToList();
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
    public async Task UpdateUserRoleAsync(int userId, string roleName, int actingUserId, CancellationToken cancellationToken = default)
    {
        if (userId == actingUserId)
        {
            throw new InvalidOperationException("Нельзя изменить роль собственной учётной записи администратора.");
        }

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

    /// <summary>
    /// Создаёт нового пользователя с указанной ролью.
    /// </summary>
    public async Task<UserListItem> CreateUserAsync(AdminUserCreateModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            throw new InvalidOperationException("Заполните обязательные поля для создания пользователя.");
        }

        var normalizedEmail = model.Email.Trim().ToUpperInvariant();
        var existing = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existing is not null)
        {
            throw new InvalidOperationException("Пользователь с таким email уже существует.");
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == model.RoleName, cancellationToken);
        if (role is null)
        {
            throw new InvalidOperationException("Выбранная роль недоступна.");
        }

        var user = new ApplicationUser
        {
            Email = model.Email.Trim(),
            UserName = model.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            NormalizedUserName = normalizedEmail,
            PhoneNumber = model.Phone,
            LastName = model.LastName.Trim(),
            FirstName = model.FirstName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(model.MiddleName) ? null : model.MiddleName.Trim(),
            RoleId = role.Id,
            RoleName = role.Name
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            var message = string.Join("; ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Не удалось создать пользователя {Email}: {Message}", model.Email, message);
            throw new InvalidOperationException(message);
        }

        if (!await _userManager.IsInRoleAsync(user, role.Name))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
            if (!roleResult.Succeeded)
            {
                var message = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Не удалось привязать роль {Role} к пользователю {Email}: {Message}", role.Name, model.Email, message);
                throw new InvalidOperationException(message);
            }
        }

        // Возвращаем данные для мгновенного отображения в таблице
        return new UserListItem
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber ?? string.Empty,
            RoleName = role.Name,
            RoleDisplayName = role.DisplayName,
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>
    /// Помечает пользователя удалённым через Identity, не давая снести самого себя.
    /// </summary>
    public async Task DeleteUserAsync(int userId, int actingUserId, CancellationToken cancellationToken = default)
    {
        if (userId == actingUserId)
        {
            throw new InvalidOperationException("Нельзя удалить собственную учётную запись администратора.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new InvalidOperationException("Пользователь не найден или уже удалён.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Не удалось удалить пользователя {UserId}: {Message}", userId, message);
            throw new InvalidOperationException(message);
        }
    }

    private static UserListItem MapToListItem(DataLayer.Models.User entity)
    {
        return new UserListItem
        {
            Id = entity.Id,
            FullName = FullNameFormatter.Combine(entity.LastName, entity.FirstName, entity.MiddleName),
            Email = entity.Email,
            Phone = entity.Phone,
            RoleName = entity.Role?.Name ?? string.Empty,
            RoleDisplayName = entity.Role?.DisplayName ?? entity.Role?.Name ?? "—",
            CreatedAt = entity.CreatedAt
        };
    }
}
