namespace WebApp.ViewModels;

/// <summary>
/// Краткое представление объекта недвижимости для списков и карточек.
/// </summary>
public record RealEstateSummary(
    int Id,
    string Address,
    string District,
    string Type,
    string Status,
    int StatusId,
    decimal Price,
    int Rooms,
    decimal Area,
    int Floor,
    int TotalFloors,
    string Agent,
    int AgentId,
    bool HasBalcony,
    bool HasParking,
    bool HasElevator,
    string? PrimaryPhoto,
    decimal? Latitude,
    decimal? Longitude,
    bool IsSold);
