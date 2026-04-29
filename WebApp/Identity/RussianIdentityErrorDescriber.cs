using Microsoft.AspNetCore.Identity;

namespace WebApp.Identity;

/// <summary>
/// Локализует стандартные сообщения Identity на русский язык,
/// чтобы пользователи получали понятные подсказки при ошибках в пароле или учётной записи.
/// </summary>
public class RussianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
        => new IdentityError
        {
            Code = nameof(DefaultError),
            Description = "Произошла неизвестная ошибка. Попробуйте ещё раз."
        };

    public override IdentityError DuplicateEmail(string email)
        => new IdentityError
        {
            Code = nameof(DuplicateEmail),
            Description = $"Пользователь с адресом {email} уже зарегистрирован."
        };

    public override IdentityError DuplicateUserName(string userName)
        => new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = $"Имя пользователя {userName} уже используется."
        };

    public override IdentityError InvalidEmail(string? email)
        => new IdentityError
        {
            Code = nameof(InvalidEmail),
            Description = $"Адрес {email} имеет неверный формат."
        };

    public override IdentityError PasswordTooShort(int length)
        => new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = $"Пароль должен содержать не меньше {length} символов."
        };

    public override IdentityError PasswordRequiresDigit()
        => new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = "Пароль должен содержать хотя бы одну цифру (0–9)."
        };

    public override IdentityError PasswordRequiresLower()
        => new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = "Пароль должен содержать хотя бы одну строчную букву."
        };

    public override IdentityError PasswordRequiresUpper()
        => new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "Пароль должен содержать хотя бы одну заглавную букву."
        };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "Пароль должен содержать хотя бы один специальный символ (например, !, #, @)."
        };
}
