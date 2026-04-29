namespace WebApp.ViewModels;

/// <summary>
/// Строка административного отчёта с категорией и количественным значением.
/// </summary>
public record AdminReportRow(string Category, int Value);
