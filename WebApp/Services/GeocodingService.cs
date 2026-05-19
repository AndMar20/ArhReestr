using System.Globalization;
using System.Net;
using System.Text.Json;

namespace WebApp.Services;

public sealed class GeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeocodingService> _logger;
    private readonly IConfiguration _configuration;

    public GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ArhReestr/1.0");
    }

    public async Task<Coordinates?> FindAsync(string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new InvalidOperationException("Укажите адрес для поиска на карте");
        }

        var city = _configuration["Geocoding:DefaultCity"] ?? "Архангельск";
        var query = address.Contains(city, StringComparison.OrdinalIgnoreCase)
            ? address
            : $"{city}, {address}";

        var url = $"https://nominatim.openstreetmap.org/search?format=json&limit=1&countrycodes=ru&q={WebUtility.UrlEncode(query)}";

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var first = document.RootElement.EnumerateArray().FirstOrDefault();
            if (first.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            var lat = first.GetProperty("lat").GetString();
            var lon = first.GetProperty("lon").GetString();

            if (!decimal.TryParse(lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
                !decimal.TryParse(lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
            {
                return null;
            }

            return new Coordinates(latitude, longitude);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Не удалось найти координаты для адреса {Address}", query);
            throw new InvalidOperationException("Сервис карты временно недоступен. Попробуйте позже.", ex);
        }
    }
}

public readonly record struct Coordinates(decimal Latitude, decimal Longitude);
