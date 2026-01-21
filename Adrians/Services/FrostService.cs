using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Adrians.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Adrians.Services;

public sealed class FrostService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;

    public FrostService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _config = config;
    }

    /// Hentar siste observerte snødybde (cm) frå Frost for ei gitt stasjon.
    public async Task<FrostSnowDepth?> GetCurrentSnowDepthAsync(
     string locationName,
     string sourceId,
     CancellationToken ct = default)
    {
        var cacheKey = $"frost:snowdepth:{sourceId}";
        if (_cache.TryGetValue(cacheKey, out FrostSnowDepth? cached))
            return cached;

        var clientId = _config["frost-client-id"];
        var clientSecret = _config["frost-client-secret"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Missing Frost credentials.");

        var client = _httpClientFactory.CreateClient("frost");
        var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", auth);

        var to = DateTimeOffset.UtcNow.Date.AddDays(1);      // i dag 00:00 → i morgon 00:00
        var from = to.AddDays(-7);                           // siste 7 dagar

        var url =
            "https://frost.met.no/observations/v0.jsonld" +
            $"?sources={Uri.EscapeDataString(sourceId)}" +
            $"&referencetime={from:O}/{to:O}" +
            $"&elements=surface_snow_thickness" +
            $"&timeresolutions=P1D";

        using var response = await client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        if (!doc.RootElement.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Array ||
            data.GetArrayLength() == 0)
            return null;

        FrostSnowDepth? latest = null;

        foreach (var item in data.EnumerateArray())
        {
            if (!item.TryGetProperty("referenceTime", out var rtEl)) continue;
            if (!DateTimeOffset.TryParse(rtEl.GetString(), out var referenceTime)) continue;
            if (!item.TryGetProperty("observations", out var obsArr)) continue;

            foreach (var obs in obsArr.EnumerateArray())
            {
                if (!obs.TryGetProperty("value", out var vEl) ||
                    vEl.ValueKind != JsonValueKind.Number)
                    continue;

                if (latest == null || referenceTime > latest.ObservedAt)
                {
                    latest = new FrostSnowDepth
                    {
                        LocationName = locationName,
                        SourceId = sourceId,
                        SnowDepthCm = vEl.GetDouble(),
                        ObservedAt = referenceTime
                    };
                }
            }
        }

        if (latest != null)
            _cache.Set(cacheKey, latest, TimeSpan.FromMinutes(30));

        return latest;
    }

}
