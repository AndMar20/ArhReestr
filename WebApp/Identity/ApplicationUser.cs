using DataLayer;
using DataLayer.Models;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Identity;

/// <summary>
/// Пользователь Identity, построенный на сущности из базы данных.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    /// <summary>
    /// Фамилия пользователя.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество пользователя, если задано.
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Полное имя, собранное для отображения.
    /// </summary>
    public string FullName => FullNameFormatter.Combine(LastName, FirstName, MiddleName);

    /// <summary>
    /// Идентификатор роли пользователя.
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Системное имя роли.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания учётной записи.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дополнительные заметки, связанные с пользователем.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Создаёт экземпляр Identity пользователя из сущности базы данных.
    /// </summary>
    public static ApplicationUser FromEntity(User entity, string roleName)
    {
        return new ApplicationUser
        {
            Id = entity.Id,
            Email = entity.Email,
            NormalizedEmail = entity.Email.ToUpperInvariant(),
            UserName = entity.Email,
            NormalizedUserName = entity.Email.ToUpperInvariant(),
            PhoneNumber = entity.Phone,
            PhoneNumberConfirmed = entity.PhoneVerified,
            EmailConfirmed = entity.EmailVerified,
            PasswordHash = entity.PasswordHash,
            LastName = entity.LastName,
            FirstName = entity.FirstName,
            MiddleName = entity.MiddleName,
            RoleId = entity.RoleId,
            RoleName = roleName,
            CreatedAt = entity.CreatedAt
        };
    }
}
