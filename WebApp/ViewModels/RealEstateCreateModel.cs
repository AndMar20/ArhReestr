using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

/// <summary>
/// Модель ввода для создания нового объекта недвижимости риелтором.
/// </summary>
public class RealEstateCreateModel
{
    [Required(ErrorMessage = "Выберите район")]
    public int? DistrictId { get; set; }

    public int? StreetId { get; set; }

    [StringLength(150, ErrorMessage = "Название улицы слишком длинное")]
    public string? NewStreetName { get; set; }

    [Required(ErrorMessage = "Укажите номер дома")]
    [StringLength(20, ErrorMessage = "Номер дома не должен превышать 20 символов")]
    public string HouseNumber { get; set; } = string.Empty;

    [Range(1, 200, ErrorMessage = "Этажность должна быть от 1 до 200")]
    public int TotalFloors { get; set; } = 5;

    public bool HasParking { get; set; }

    public bool HasElevator { get; set; }

    [Range(1800, 2100, ErrorMessage = "Год постройки должен быть в разумных пределах")]
    public int? BuildingYear { get; set; }

    [Range(-90, 90, ErrorMessage = "Широта должна быть в диапазоне [-90; 90]")]
    public decimal? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Долгота должна быть в диапазоне [-180; 180]")]
    public decimal? Longitude { get; set; }

    [Required(ErrorMessage = "Укажите тип недвижимости")]
    public int? TypeId { get; set; }

    [Range(1, 1000000000, ErrorMessage = "Цена должна быть больше нуля")]
    public decimal Price { get; set; } = 1000000m;

    [Range(1, 50, ErrorMessage = "Количество комнат должно быть больше нуля")]
    public int Rooms { get; set; } = 1;

    [Range(1, 10000, ErrorMessage = "Площадь должна быть больше нуля")]
    public decimal Area { get; set; } = 30;

    [Range(1, 200, ErrorMessage = "Этаж должен быть положительным")]
    public int Floor { get; set; } = 1;

    public bool HasBalcony { get; set; }

    [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
    public string? Description { get; set; }
}
