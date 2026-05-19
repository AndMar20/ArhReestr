namespace WebApp.ViewModels;

public sealed record AddressSuggestion(
    int HouseId,
    int DistrictId,
    string DistrictName,
    int StreetId,
    string StreetName,
    string HouseNumber,
    int TotalFloors,
    bool HasParking,
    bool HasElevator,
    int? BuildingYear,
    decimal? Latitude,
    decimal? Longitude)
{
    public string DisplayName => $"{DistrictName}, {StreetName}, д. {HouseNumber}";
}
