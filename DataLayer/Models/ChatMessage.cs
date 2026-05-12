namespace DataLayer.Models;

/// <summary>
/// Сообщение в чате между клиентом и риелтором по конкретному объекту.
/// </summary>
public class ChatMessage
{
    public int Id { get; set; }
    public int RealEstateId { get; set; }
    public int SenderId { get; set; }
    public int RecipientId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public virtual RealEstate? RealEstate { get; set; }
    public virtual User? Sender { get; set; }
    public virtual User? Recipient { get; set; }
}
