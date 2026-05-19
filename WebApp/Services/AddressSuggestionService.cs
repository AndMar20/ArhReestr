using DataLayer;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebApp.ViewModels;

namespace WebApp.Services;

public sealed class AddressSuggestionService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;

    public AddressSuggestionService(IDbContextFactory<ArhReestrContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<AddressSuggestion>> SearchHousesAsync(string query, int limit = 8, CancellationToken cancellationToken = default)
    {
        var normalized = query?.Trim() ?? string.Empty;
        if (normalized.Length < 2)
        {
            return Array.Empty<AddressSuggestion>();
        }

        limit = Math.Clamp(limit, 1, 20);
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var parsed = ParseQuery(normalized);
        var tokens = parsed.Tokens
            .Where(t => t.Length > 1 || char.IsDigit(t[0]))
            .Take(6)
            .ToList();

        if (tokens.Count == 0)
        {
            return Array.Empty<AddressSuggestion>();
        }

        var houses = await context.Houses
            .AsNoTracking()
            .Include(h => h.District)
            .Include(h => h.Street)
            .Where(h => h.Street != null && h.District != null)
            .OrderBy(h => h.District!.Name)
            .ThenBy(h => h.Street!.Name)
            .ThenBy(h => h.Number)
            .Take(1000)
            .Select(h => new AddressSuggestion(
                h.Id,
                h.DistrictId,
                h.District!.Name,
                h.StreetId,
                h.Street!.Name,
                h.Number,
                h.TotalFloors,
                h.HasParking,
                h.HasElevator,
                h.BuildingYear,
                h.Latitude,
                h.Longitude))
            .ToListAsync(cancellationToken);

        return houses
            .Select(h => new { Suggestion = h, Score = Score(h, parsed) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Suggestion.DistrictName)
            .ThenBy(x => x.Suggestion.StreetName)
            .ThenBy(x => x.Suggestion.HouseNumber)
            .Take(limit)
            .Select(x => x.Suggestion)
            .ToList();
    }

    private static AddressQuery ParseQuery(string query)
    {
        var normalized = NormalizeAddressText(query);
        var tokens = normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token is not ("ул" or "улица" or "д" or "дом"))
            .ToList();

        var houseNumber = tokens.LastOrDefault(token => token.Any(char.IsDigit));
        return new AddressQuery(tokens, houseNumber);
    }

    private static int Score(AddressSuggestion suggestion, AddressQuery query)
    {
        var district = NormalizeAddressText(suggestion.DistrictName);
        var street = NormalizeAddressText(suggestion.StreetName);
        var house = NormalizeAddressText(suggestion.HouseNumber);
        var full = $"{district} {street} {house}";
        var score = 0;

        foreach (var token in query.Tokens)
        {
            if (string.Equals(token, query.HouseNumber, StringComparison.OrdinalIgnoreCase))
            {
                if (house == token)
                {
                    score += 45;
                }
                else if (house.StartsWith(token, StringComparison.OrdinalIgnoreCase))
                {
                    score += 18;
                }
                else
                {
                    score -= 25;
                }

                continue;
            }

            if (district.Split(' ').Contains(token))
            {
                score += 25;
            }
            else if (street.Split(' ').Contains(token))
            {
                score += 25;
            }
            else if (full.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                score += 10;
            }
            else
            {
                score -= 6;
            }
        }

        if (!string.IsNullOrWhiteSpace(query.HouseNumber) && house != query.HouseNumber)
        {
            score -= 20;
        }

        return score;
    }

    private static string NormalizeAddressText(string value)
    {
        var lower = value.Trim().ToLowerInvariant().Replace('ё', 'е');
        lower = Regex.Replace(lower, @"[.,;:()\[\]""'`]+", " ");
        lower = Regex.Replace(lower, @"[-/\\]+", " ");
        lower = Regex.Replace(lower, @"\s+", " ");
        return lower.Trim();
    }

    private sealed record AddressQuery(IReadOnlyList<string> Tokens, string? HouseNumber);
}
