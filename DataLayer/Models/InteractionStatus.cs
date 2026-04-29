namespace DataLayer.Models;

/// <summary>
/// Статус взаимодействия клиента с объектом недвижимости.
/// </summary>
public class InteractionStatus
{
    /// <summary>
    /// Идентификатор статуса.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Человекочитаемое название статуса (например, «новый», «закрыт»).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Все взаимодействия, которым присвоен этот статус.
    /// </summary>
    public virtual ICollection<Interaction> Interactions { get; set; } = new HashSet<Interaction>();
}
