namespace DataLayer.Models;

/// <summary>
/// Публичный жизненный цикл объявления об объекте недвижимости.
/// </summary>
public class RealEstateStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public virtual ICollection<RealEstate> RealEstates { get; set; } = new HashSet<RealEstate>();
}
