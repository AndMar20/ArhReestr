using System.Linq;

namespace DataLayer;

/// <summary>
/// Утилита для форматирования ФИО из отдельных частей.
/// </summary>
public static class FullNameFormatter
{
    /// <summary>
    /// Склеивает ФИО, убирая пустые части и лишние пробелы между словами.
    /// </summary>
    public static string Combine(string? lastName, string? firstName, string? middleName)
    {
        return string.Join(" ", new[] { lastName, firstName, middleName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim()));
    }
}
