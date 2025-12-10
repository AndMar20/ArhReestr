using DataLayer;
using DataLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
usin WebApp.Infrastructure;

namespace WebApp.Identity;

/// <summary>
/// Пользовательское хранилище Identity, работающее поверх контекста EF Core и таблицы Users.
/// </summary>
public class ArhUserStore :
    IUserStore<ApplicationUser>,
    IUserPasswordStore<ApplicationUser>,
    IUserEmailStore<ApplicationUser>,
    IUserPhoneNumberStore<ApplicationUser>,
    IUserRoleStore<ApplicationUser>
{
    private readonly ArhReestrContext _context;
    private readonly TimeProvider _timeProvider;

    public ArhUserStore(ArhReestrContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public void Dispose()
    {
    }

    /// <summary>
    /// Преобразует сущность базы данных в пользовательскую модель Identity.
    /// </summary>
    private static ApplicationUser Map(User entity)
    {
        var roleName = entity.Role?.Name ?? string.Empty;
        return ApplicationUser.FromEntity(entity, roleName);
    }

    /// <summary>
    /// Загружает сущность пользователя вместе с ролью.
    /// </summary>
    private async Task<User?> LoadEntityAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Создаёт нового пользователя и привязывает к роли.
    /// </summary>
    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId, cancellationToken);
        if (role is null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = nameof(IdentityError),
                Description = $"Роль с идентификатором {user.RoleId} не найдена"
            });
        }

        var entity = new User
        {
            LastName = user.LastName,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber ?? string.Empty,
            PasswordHash = user.PasswordHash ?? string.Empty,
            RoleId = user.RoleId,
            CreatedAt = _timeProvider.GetMoscowDateTime(),
            PhoneVerified = user.PhoneNumberConfirmed,
            EmailVerified = user.EmailConfirmed
        };

        _context.Users.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        user.Id = entity.Id;
        user.RoleName = role.Name;
        user.CreatedAt = entity.CreatedAt;
        return IdentityResult.Success;
    }

    /// <summary>
    /// Помечает пользователя как удалённого.
    /// </summary>
    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var entity = await LoadEntityAsync(user.Id, cancellationToken);
        if (entity is null)
        {
            return IdentityResult.Success;
        }

        entity.DeletedAt = _timeProvider.GetMoscowDateTime();
        await _context.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    /// <summary>
    /// Возвращает пользователя по идентификатору.
    /// </summary>
    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(userId, out var id))
        {
            return null;
        }

        var entity = await LoadEntityAsync(id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    /// <summary>
    /// Находит пользователя по нормализованному имени (email).
    /// </summary>
    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var entity = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToUpper() == normalizedUserName, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName ?? string.Empty);
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email ?? string.Empty);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Обновляет данные пользователя.
    /// </summary>
    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var entity = await LoadEntityAsync(user.Id, cancellationToken);
        if (entity is null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = nameof(IdentityError),
                Description = "Пользователь не найден"
            });
        }

        entity.LastName = user.LastName;
        entity.FirstName = user.FirstName;
        entity.MiddleName = user.MiddleName;
        entity.Email = user.Email ?? entity.Email;
        entity.Phone = user.PhoneNumber ?? entity.Phone;
        entity.PasswordHash = user.PasswordHash ?? entity.PasswordHash;
        entity.RoleId = user.RoleId;
        entity.EmailVerified = user.EmailConfirmed;
        entity.PhoneVerified = user.PhoneNumberConfirmed;

        await _context.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        user.UserName = email;
        user.NormalizedEmail = email?.ToUpperInvariant();
        user.NormalizedUserName = user.NormalizedEmail;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var entity = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToUpper() == normalizedEmail, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail ?? user.Email?.ToUpperInvariant());
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task SetPhoneNumberAsync(ApplicationUser user, string? phoneNumber, CancellationToken cancellationToken)
    {
        user.PhoneNumber = phoneNumber;
        return Task.CompletedTask;
    }

    public Task<string?> GetPhoneNumberAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PhoneNumber);
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PhoneNumberConfirmed);
    }

    public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.PhoneNumberConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        if (role is null)
        {
            throw new InvalidOperationException($"Роль {roleName} не найдена");
        }

        user.RoleId = role.Id;
        user.RoleName = role.Name;
        var result = await UpdateAsync(user, cancellationToken);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(';', result.Errors.Select(e => e.Description)));
        }
    }

    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        if (string.Equals(user.RoleName, roleName, StringComparison.OrdinalIgnoreCase))
        {
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "client", cancellationToken);
            if (defaultRole is not null)
            {
                user.RoleId = defaultRole.Id;
                user.RoleName = defaultRole.Name;
                await UpdateAsync(user, cancellationToken);
            }
        }
    }

    public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        IList<string> roles = string.IsNullOrWhiteSpace(user.RoleName)
            ? Array.Empty<string>()
            : new[] { user.RoleName };
        return Task.FromResult(roles);
    }

    public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        var isInRole = string.Equals(user.RoleName, roleName, StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(isInRole);
    }

    /// <summary>
    /// Возвращает всех пользователей, относящихся к заданной роли.
    /// </summary>
    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var entities = await _context.Users.Include(u => u.Role)
            .Where(u => u.Role != null && u.Role.Name == roleName)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToList();
    }
}
