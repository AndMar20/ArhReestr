namespace DataLayer.Models;

/// <summary>
/// Закрытая сделка, созданная из заявки после успешного завершения.
/// </summary>
public class Deal
{
    public int Id { get; set; }
    public int InteractionId { get; set; }
    public int RealEstateId { get; set; }
    public int AgentId { get; set; }
    public int ClientId { get; set; }
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }
    public DateTime ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Interaction? Interaction { get; set; }
    public virtual RealEstate? RealEstate { get; set; }
    public virtual User? Agent { get; set; }
    public virtual User? Client { get; set; }
}
