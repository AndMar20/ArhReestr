using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

/// <summary>
/// Модель фильтрации списка объектов недвижимости с базовыми проверками ввода.
/// </summary>
public class RealEstateFilterModel : IValidatableObject
{
    private const int MaxPageSize = 200;

    public int? DistrictId { get; set; }

    public int? TypeId { get; set; }

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Цена не может быть отрицательной")]
    public decimal? MinPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Цена не может быть отрицательной")]
    public decimal? MaxPrice { get; set; }

    [Range(typeof(decimal), "0", "10000", ErrorMessage = "Площадь должна быть неотрицательной")]
    public decimal? MinArea { get; set; }

    [Range(typeof(decimal), "0", "10000", ErrorMessage = "Площадь должна быть неотрицательной")]
    public decimal? MaxArea { get; set; }

    [Range(0, 50, ErrorMessage = "Количество комнат не может быть отрицательным")]
    public int? Rooms { get; set; }

    public bool? HasBalcony { get; set; }

    public bool? HasParking { get; set; }

    public bool? HasElevator { get; set; }

    public string SortBy { get; set; } = "price";

    public bool SortDescending { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть положительным")]
    public int Page { get; set; } = 1;

    [Range(1, MaxPageSize, ErrorMessage = "Размер страницы должен быть в диапазоне 1-200 записей")]
    public int PageSize { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinPrice is not null && MaxPrice is not null && MinPrice > MaxPrice)
        {
            yield return new ValidationResult("Минимальная цена не может превышать максимальную", new[] { nameof(MinPrice), nameof(MaxPrice) });
        }

        if (MinArea is not null && MaxArea is not null && MinArea > MaxArea)
        {
            yield return new ValidationResult("Минимальная площадь не может превышать максимальную", new[] { nameof(MinArea), nameof(MaxArea) });
        }
    }
}
