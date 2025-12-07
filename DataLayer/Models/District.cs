namespace DataLayer.Models;

/// <summary>
/// Район города, к которому относятся объекты недвижимости.
/// </summary>
public class District
{
    /// <summary>
    /// Первичный ключ записи района.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название района, отображаемое пользователям.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Навигационное свойство для домов, находящихся в этом районе.
    /// </summary>
    public virtual ICollection<House> Houses { get; set; } = new HashSet<House>();
}
