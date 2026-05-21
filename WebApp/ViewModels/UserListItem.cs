namespace WebApp.ViewModels;

/// <summary>
/// Краткие сведения о пользователе для административных списков.
/// </summary>
public class UserListItem
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Полное имя, собранное из ФИО.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email, используемый для входа.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Контактный телефон.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Системное имя роли.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Человекочитаемое имя роли.
    /// </summary>
    public string RoleDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания учетной записи.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата блокировки учетной записи.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Признак заблокированного пользователя.
    /// </summary>
    public bool IsBlocked => DeletedAt.HasValue;
}