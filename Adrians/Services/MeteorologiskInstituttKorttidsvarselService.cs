using Microsoft.Extensions.Caching.Memory;
using Adrians.ViewModels;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Adrians.Services
{
    public class MeteorologiskInstituttKorttidsvarselService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;

        public MeteorologiskInstituttKorttidsvarselService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public async Task<KorttidsvarselViewModel?> HentKorttidsvarselAsync(string locationName, double lat, double lon,
            CancellationToken ct = default)
        {
            var cacheKey = $"nowcast:{locationName}:{lat:F5}:{lon:F5}";

            if (_cache.TryGetValue(cacheKey, out KorttidsvarselViewModel? cached))
                return cached;

            var client = _httpClientFactory.CreateClient("met.no");
            var url = $"https://api.met.no/weatherapi/nowcast/2.0/complete?lat={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await client.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            // JSON-struktur er "properties.timeseries[0]"
            if (!document.RootElement.TryGetProperty("properties", out var props)) return null;
            if (!props.TryGetProperty("timeseries", out var ts) || ts.GetArrayLength() == 0) return null;

            var first = ts[0];

            DateTimeOffset updatedAt = DateTimeOffset.UtcNow;

            if (first.TryGetProperty("time", out var timeEl) &&
                DateTimeOffset.TryParse(timeEl.GetString(), out var parsed))
            {
                updatedAt = parsed;
            }

            double? temp = null, wind = null, gust = null, precip1h = null;
            string? symbol = null;

            if (first.TryGetProperty("data", out var data) &&
                data.TryGetProperty("instant", out var instant) &&
                instant.TryGetProperty("details", out var details))
            {
                if (details.TryGetProperty("air_temperature", out var tempEl))
                    temp = tempEl.GetDouble();
                if (details.TryGetProperty("wind_speed", out var windEl))
                    wind = windEl.GetDouble();
                if (details.TryGetProperty("wind_speed_of_gust", out var gustEl))
                    gust = gustEl.GetDouble();
            }

            if (first.TryGetProperty("data", out var data2)
                && data2.TryGetProperty("next_1_hours", out var next1h))
            {
                if (next1h.TryGetProperty("details", out var d1)
                    && d1.TryGetProperty("precipitation_amount", out var p))
                {
                    precip1h = p.GetDouble();
                }

                if (next1h.TryGetProperty("summary", out var s)
                    && s.TryGetProperty("symbol_code", out var sc))
                {
                    symbol = sc.GetString();
                }
            }

            var korttidsvarsel = new KorttidsvarselViewModel
            {
                LocationName = locationName,
                UpdatedAt = updatedAt,
                TemperatureC = temp,
                WindSpeedMs = wind,
                WindGustMs = gust,
                PrecipNext1hMm = precip1h,
                SymbolCode = symbol
            };

            _cache.Set(cacheKey, korttidsvarsel, TimeSpan.FromSeconds(60));
            return korttidsvarsel;
        }
    }
}
