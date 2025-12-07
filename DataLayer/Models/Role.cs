namespace DataLayer.Models;

/// <summary>
/// Роль пользователя, определяющая права в системе.
/// </summary>
public class Role
{
    /// <summary>
    /// Идентификатор роли.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Системное имя роли, используемое в проверках авторизации.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое название роли.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Пользователи, которым назначена эта роль.
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
}
