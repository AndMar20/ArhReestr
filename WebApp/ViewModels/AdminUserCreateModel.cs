using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

/// <summary>
/// Модель для добавления пользователя администратором.
/// </summary>
public class AdminUserCreateModel
{
    [Required(ErrorMessage = "Укажите фамилию")]
    [StringLength(100, ErrorMessage = "Фамилия слишком длинная")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите имя")]
    [StringLength(100, ErrorMessage = "Имя слишком длинное")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Отчество слишком длинное")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Нужен email")]
    [EmailAddress(ErrorMessage = "Укажите корректный email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Укажите корректный телефон")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Задайте пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 символов")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль")]
    public string RoleName { get; set; } = string.Empty;
}
