namespace DataLayer.Models;

/// <summary>
/// Запись об объекте недвижимости в реестре.
/// </summary>
public class RealEstate
{
    /// <summary>
    /// Идентификатор объявления или объекта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Пользователь-агент, создавший объект.
    /// </summary>
    public int AgentId { get; set; }

    /// <summary>
    /// Тип недвижимости (квартира, дом и т.д.).
    /// </summary>
    public int TypeId { get; set; }

    /// <summary>
    /// Дом, к которому относится объект.
    /// </summary>
    public int HouseId { get; set; }

    /// <summary>
    /// Дополнительное описание объекта, которое видит пользователь.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Цена объекта в валюте базы данных.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Количество комнат.
    /// </summary>
    public int Rooms { get; set; }

    /// <summary>
    /// Общая площадь объекта в квадратных метрах.
    /// </summary>
    public decimal Area { get; set; }

    /// <summary>
    /// Этаж, на котором расположен объект.
    /// </summary>
    public int Floor { get; set; }

    /// <summary>
    /// Наличие балкона у объекта.
    /// </summary>
    public bool HasBalcony { get; set; }

    /// <summary>
    /// Дата и время создания записи.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Время удаления записи, если она была помечена как удалённая.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Навигационное свойство пользователя-агента.
    /// </summary>
    public virtual User? Agent { get; set; }

    /// <summary>
    /// Навигационное свойство типа объекта.
    /// </summary>
    public virtual RealEstateType? Type { get; set; }

    /// <summary>
    /// Навигационное свойство дома.
    /// </summary>
    public virtual House? House { get; set; }

    /// <summary>
    /// Фотографии, связанные с объектом.
    /// </summary>
    public virtual ICollection<RealEstatePhoto> Photos { get; set; } = new HashSet<RealEstatePhoto>();

    /// <summary>
    /// История взаимодействий клиентов по данному объекту.
    /// </summary>
    public virtual ICollection<Interaction> Interactions { get; set; } = new HashSet<Interaction>();
}
