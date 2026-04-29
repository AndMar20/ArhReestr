namespace DataLayer.Models;

/// <summary>
/// Тип объекта недвижимости (квартира, дом, офис и т.д.).
/// </summary>
public class RealEstateType
{
    /// <summary>
    /// Первичный ключ типа объекта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название типа, отображаемое пользователям.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Объекты недвижимости, относящиеся к данному типу.
    /// </summary>
    public virtual ICollection<RealEstate> RealEstates { get; set; } = new HashSet<RealEstate>();
}
