namespace DataLayer.Models;

/// <summary>
/// Улица населённого пункта, содержащая дома.
/// </summary>
public class Street
{
    /// <summary>
    /// Идентификатор улицы в базе данных.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название улицы, которое отображается в интерфейсе.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Коллекция домов, расположенных на этой улице.
    /// </summary>
    public virtual ICollection<House> Houses { get; set; } = new HashSet<House>();
}
