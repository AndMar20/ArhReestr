using DataLayer.Models;

namespace DataLayer;

/// <summary>
/// Утилита для построения читабельных адресов по данным дома.
/// </summary>
public static class AddressFormatter
{
    /// <summary>
    /// Формирует компактный адрес по данным дома, подставляя название улицы и номер дома.
    /// </summary>
    public static string Format(House? house)
    {
        if (house is null)
        {
            return string.Empty;
        }

        var streetName = house.Street?.Name ?? string.Empty;
        var number = house.Number;

        if (string.IsNullOrWhiteSpace(streetName))
        {
            return number;
        }

        // Если номер не указан, возвращаем только название улицы, иначе «улица, д. номер».
        return string.IsNullOrWhiteSpace(number) ? streetName : $"{streetName}, д. {number}";
    }
}
