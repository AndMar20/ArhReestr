using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebApp.ViewModels;

/// <summary>
/// Данные, которые пользователь вводит при регистрации.
/// </summary>
public class RegisterInputModel
{
    [Required(ErrorMessage = "Фамилия обязательна")]
    [MaxLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Имя обязательно")]
    [MaxLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Отчество не должно превышать 50 символов")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Введите корректный email")]
    public string Email { get; set; } = string.Empty;

    private string phone = string.Empty;

    [Required(ErrorMessage = "Телефон обязателен")]
    [Phone(ErrorMessage = "Введите корректный номер телефона")]
    [RegularExpression(
        @"^[0-9+()\-\s]{10,20}$",
        ErrorMessage = "Телефон должен содержать 10–20 цифр и может включать +, пробелы, скобки и дефис")]
    public string Phone
    {
        get => phone;
        set => phone = Regex.Replace(value ?? string.Empty, "[^0-9+()\\-\\s]", string.Empty);
    }

    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
        ErrorMessage = "Пароль должен содержать заглавную, строчную буквы и цифру")]
    public string Password { get; set; } = string.Empty;
}
