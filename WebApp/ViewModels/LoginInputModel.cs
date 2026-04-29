using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

/// <summary>
/// Данные, вводимые пользователем при авторизации.
/// </summary>
public class LoginInputModel
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Введите корректный email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
