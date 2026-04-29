using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

/// <summary>
/// Модель ввода для обновления существующего объекта недвижимости.
/// </summary>
public class RealEstateUpdateModel : RealEstateCreateModel
{
    /// <summary>
    /// Идентификатор редактируемого объекта.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Некорректный идентификатор объекта")]
    public int Id { get; set; }
}
