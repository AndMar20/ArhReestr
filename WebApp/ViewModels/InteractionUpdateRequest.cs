using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

/// <summary>
/// Запрос на обновление статуса обращения по объекту недвижимости.
/// </summary>
public class InteractionUpdateRequest
{
    [Required]
    public int InteractionId { get; set; }

    [Required]
    public int StatusId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
