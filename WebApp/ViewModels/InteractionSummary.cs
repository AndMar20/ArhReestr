namespace WebApp.ViewModels;

/// <summary>
/// Краткое представление обращения клиента по объекту недвижимости.
/// </summary>
public record InteractionSummary(
    int Id,
    int ClientId,
    string Client,
    string Agent,
    int AgentId,
    int RealEstateId,
    string RealEstate,
    int StatusId,
    string Status,
    DateTime ContactedAt,
    DateTime UpdatedAt,
    string? Notes)
{
    public string ClientPhone { get; init; } = string.Empty;
    public string AgentPhone { get; init; } = string.Empty;
};
