using Microsoft.AspNetCore.Identity;

namespace WebApp.Identity;

/// <summary>
/// Роль приложения, расширенная человекочитаемым названием.
/// </summary>
public class ApplicationRole : IdentityRole<int>
{
    /// <summary>
    /// Отображаемое название роли.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}
