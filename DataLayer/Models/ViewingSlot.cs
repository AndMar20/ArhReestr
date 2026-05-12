namespace DataLayer.Models;

/// <summary>
/// Слот показа объекта для календаря встреч.
/// </summary>
public class ViewingSlot
{
    public int Id { get; set; }
    public int RealEstateId { get; set; }
    public int AgentId { get; set; }
    public int? ClientId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Status { get; set; } = "available";
    public string? Notes { get; set; }

    public virtual RealEstate? RealEstate { get; set; }
    public virtual User? Agent { get; set; }
    public virtual User? Client { get; set; }
}
