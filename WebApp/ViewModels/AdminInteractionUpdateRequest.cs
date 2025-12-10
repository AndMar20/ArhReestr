using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

/// <summary>
/// Запрос на обновление обращения администратором с возможностью смены агента и комментария.
/// </summary>
public class AdminInteractionUpdateRequest
{
    [Required]
    public int InteractionId { get; set; }

    [Required]
    public int StatusId { get; set; }

    [Required]
    public int AgentId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
