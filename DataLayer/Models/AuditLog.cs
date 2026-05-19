namespace DataLayer.Models;

/// <summary>
/// Запись истории действий пользователей и системных изменений.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public int? ActorUserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User? ActorUser { get; set; }
}
