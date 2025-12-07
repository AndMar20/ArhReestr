namespace DataLayer.Models;

/// <summary>
/// Жилой дом, которому принадлежат квартиры или нежилые помещения.
/// </summary>
public class House
{
    /// <summary>
    /// Идентификатор дома.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ссылка на улицу, на которой расположен дом.
    /// </summary>
    public int StreetId { get; set; }

    /// <summary>
    /// Район города, к которому относится дом.
    /// </summary>
    public int DistrictId { get; set; }

    /// <summary>
    /// Номер дома (например, «12А»).
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Количество этажей в доме.
    /// </summary>
    public int TotalFloors { get; set; }

    /// <summary>
    /// Признак наличия парковки.
    /// </summary>
    public bool HasParking { get; set; }

    /// <summary>
    /// Признак наличия лифта.
    /// </summary>
    public bool HasElevator { get; set; }

    /// <summary>
    /// Год постройки дома, если он известен.
    /// </summary>
    public int? BuildingYear { get; set; }

    /// <summary>
    /// Географическая широта здания.
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Географическая долгота здания.
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Навигационное свойство к улице.
    /// </summary>
    public virtual Street? Street { get; set; }

    /// <summary>
    /// Навигационное свойство района.
    /// </summary>
    public virtual District? District { get; set; }

    /// <summary>
    /// Коллекция объектов недвижимости, находящихся в этом доме.
    /// </summary>
    public virtual ICollection<RealEstate> RealEstates { get; set; } = new HashSet<RealEstate>();
}
