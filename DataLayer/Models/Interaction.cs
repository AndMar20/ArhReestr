namespace DataLayer.Models;

/// <summary>
/// Взаимодействие клиента и агента по конкретному объекту недвижимости.
/// </summary>
public class Interaction
{
    /// <summary>
    /// Идентификатор записи взаимодействия.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Клиент, инициировавший взаимодействие.
    /// </summary>
    public int ClientId { get; set; }

    /// <summary>
    /// Агент, отвечающий за объект.
    /// </summary>
    public int AgentId { get; set; }

    /// <summary>
    /// Объект недвижимости, по которому состоялось общение.
    /// </summary>
    public int RealEstateId { get; set; }

    /// <summary>
    /// Статус взаимодействия.
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Дата и время первого контакта.
    /// </summary>
    public DateTime ContactedAt { get; set; }

    /// <summary>
    /// Дата и время последнего обновления статуса.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Дополнительные заметки по общению.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Дата логического удаления записи.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Навигационное свойство клиента.
    /// </summary>
    public virtual User? Client { get; set; }

    /// <summary>
    /// Навигационное свойство агента.
    /// </summary>
    public virtual User? Agent { get; set; }

    /// <summary>
    /// Навигационное свойство объекта недвижимости.
    /// </summary>
    public virtual RealEstate? RealEstate { get; set; }

    /// <summary>
    /// Навигационное свойство статуса взаимодействия.
    /// </summary>
    public virtual InteractionStatus? Status { get; set; }
}
