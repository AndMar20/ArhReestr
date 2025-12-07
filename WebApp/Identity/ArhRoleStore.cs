using DataLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Identity;

/// <summary>
/// Хранилище ролей Identity, работающее поверх контекста EF Core.
/// </summary>
public class ArhRoleStore : IRoleStore<ApplicationRole>
{
    private readonly ArhReestrContext _context;

    public ArhRoleStore(ArhReestrContext context)
    {
        _context = context;
    }

    public void Dispose()
    {
    }

    /// <summary>
    /// Создаёт новую роль в базе данных.
    /// </summary>
    public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        _context.Roles.Add(new DataLayer.Models.Role
        {
            Name = role.Name ?? string.Empty,
            DisplayName = role.DisplayName
        });

        await _context.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    /// <summary>
    /// Обновляет существующую роль.
    /// </summary>
    public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        var entity = await _context.Roles.FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
        if (entity is null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Роль не найдена" });
        }

        entity.Name = role.Name ?? entity.Name;
        entity.DisplayName = role.DisplayName;
        await _context.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    /// <summary>
    /// Удаляет роль, если она существует.
    /// </summary>
    public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        var entity = await _context.Roles.FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
        if (entity is null)
        {
            return IdentityResult.Success;
        }

        _context.Roles.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    /// <summary>
    /// Возвращает строковый идентификатор роли.
    /// </summary>
    public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id.ToString());
    }

    /// <summary>
    /// Возвращает имя роли.
    /// </summary>
    public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name ?? string.Empty);
    }

    /// <summary>
    /// Обновляет имя роли в модели.
    /// </summary>
    public Task SetRoleNameAsync(ApplicationRole role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Возвращает нормализованное имя роли.
    /// </summary>
    public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName ?? string.Empty);
    }

    /// <summary>
    /// Устанавливает нормализованное имя роли.
    /// </summary>
    public Task SetNormalizedRoleNameAsync(ApplicationRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ищет роль по идентификатору.
    /// </summary>
    public async Task<ApplicationRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(roleId, out var id))
        {
            return null;
        }

        var entity = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        return new ApplicationRole
        {
            Id = entity.Id,
            Name = entity.Name,
            NormalizedName = entity.Name.ToUpperInvariant(),
            DisplayName = entity.DisplayName
        };
    }

    /// <summary>
    /// Ищет роль по нормализованному имени.
    /// </summary>
    public async Task<ApplicationRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var entity = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToUpper() == normalizedRoleName, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        return new ApplicationRole
        {
            Id = entity.Id,
            Name = entity.Name,
            NormalizedName = normalizedRoleName,
            DisplayName = entity.DisplayName
        };
    }
}
