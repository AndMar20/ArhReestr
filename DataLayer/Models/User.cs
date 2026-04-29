using DataLayer;

namespace DataLayer.Models;

/// <summary>
/// Учетная запись пользователя системы (агента или клиента).
/// </summary>
public class User
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Фамилия пользователя.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество пользователя, если указано.
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Контактный номер телефона.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Электронная почта, используется как логин.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Хэш пароля, хранимый в базе.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор роли пользователя.
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Дата создания записи.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата логического удаления учетной записи.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Подтверждён ли номер телефона.
    /// </summary>
    public bool PhoneVerified { get; set; }

    /// <summary>
    /// Подтверждена ли почта.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Навигационное свойство роли пользователя.
    /// </summary>
    public virtual Role? Role { get; set; }

    /// <summary>
    /// Объявления об объектах недвижимости, созданные пользователем-агентом.
    /// </summary>
    public virtual ICollection<RealEstate> RealEstates { get; set; } = new HashSet<RealEstate>();

    /// <summary>
    /// Взаимодействия, где пользователь выступает клиентом.
    /// </summary>
    public virtual ICollection<Interaction> ClientInteractions { get; set; } = new HashSet<Interaction>();

    /// <summary>
    /// Взаимодействия, где пользователь выступает агентом.
    /// </summary>
    public virtual ICollection<Interaction> AgentInteractions { get; set; } = new HashSet<Interaction>();

    /// <summary>
    /// Формирует полное имя пользователя для вывода в интерфейсе.
    /// </summary>
    public string GetFullName() => FullNameFormatter.Combine(LastName, FirstName, MiddleName);
}
