namespace WebApp.ViewModels;

/// <summary>
/// Универсальная модель результата с пагинацией.
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
