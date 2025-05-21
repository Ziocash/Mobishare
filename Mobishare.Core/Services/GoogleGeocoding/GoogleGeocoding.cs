using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mobishare.App.Services;

namespace Mobishare.Core.Services.GoogleGeocoding;

public class GoogleGeocodingService : IGoogleGeocodingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleGeocodingService> _logger;

    public GoogleGeocodingService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GoogleGeocodingService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string?> GetAddressFromCoordinatesAsync(double latitude, double longitude)
    {
        var apiKey = _configuration["GoogleMaps:ApiKey"];
        var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}&key={apiKey}";

        _logger.LogInformation("Requesting geolocation for coordinates: {Lat}, {Lng}", latitude, longitude);

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch geolocation data. StatusCode: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            {
                _logger.LogInformation("No geolocation results found for coordinates: {Lat}, {Lng}", latitude, longitude);
                return null;
            }

            var formattedAddress = results[0].GetProperty("formatted_address").GetString();

            _logger.LogInformation("Resolved address: {Address}", formattedAddress);
            return formattedAddress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving address for coordinates: {Lat}, {Lng}", latitude, longitude);
            return null;
        }
    }
}
