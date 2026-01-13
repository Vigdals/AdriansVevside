using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Adrians.Services;

public class FrostService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;

    public FrostService(IHttpClientFactory httpClientFactory, IMemoryCache cache, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _config = config;
    }

    public async Task<FrostSnowSummary?> GetSnowSummaryAsync(
        string locationName,
        string sourceId,
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        // Cache 10 min er plenty for Frost
        var cacheKey = $"frost:snow:{locationName}:{sourceId}:{now:yyyyMMddHH}";
        if (_cache.TryGetValue(cacheKey, out FrostSnowSummary? cached))
            return cached;

        var clientId = _config["frost-client-id"];
        var clientSecret = _config["frost-client-secret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Missing Frost credentials (frost-client-id / frost-client-secret).");

        var client = _httpClientFactory.CreateClient("frost");

        // Basic auth header
        var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

        // 48t vindauge (du kan også køyre 24t)
        var from48 = now.AddHours(-48);
        var from24 = now.AddHours(-24);

        // 1) Snødybde per dag (P1D)
        var snowDaily = await GetObservationsAsync(
            client,
            sourceId,
            from48,
            now,
            "surface_snow_thickness",
            timeResolution: "P1D",
            ct);

        // 2) Nedbør sum per time (vi summerer i appen)
        // element: sum(precipitation_amount PT1H)
        var precip48 = await GetObservationsAsync(
            client,
            sourceId,
            from48,
            now,
            "sum(precipitation_amount PT1H)",
            timeResolution: null,
            ct);

        var precip24 = await GetObservationsAsync(
            client,
            sourceId,
            from24,
            now,
            "sum(precipitation_amount PT1H)",
            timeResolution: null,
            ct);

        double? snowNowCm = snowDaily
            .OrderByDescending(x => x.ReferenceTime)
            .Select(x => x.Value)
            .FirstOrDefault();

        double? snow48AgoCm = snowDaily
            .OrderBy(x => x.ReferenceTime)
            .Select(x => x.Value)
            .FirstOrDefault();

        var sum48 = precip48.Sum(x => x.Value ?? 0);
        var sum24 = precip24.Sum(x => x.Value ?? 0);

        var result = new FrostSnowSummary
        {
            LocationName = locationName,
            SourceId = sourceId,
            SnowDepthNowCm = snowNowCm,
            SnowDepth48hAgoCm = snow48AgoCm,
            SnowDepthDelta48hCm = (snowNowCm.HasValue && snow48AgoCm.HasValue) ? snowNowCm - snow48AgoCm : null,
            PrecipLast24hMm = sum24,
            PrecipLast48hMm = sum48
        };

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    private async Task<List<FrostPoint>> GetObservationsAsync(
        HttpClient client,
        string sourceId,
        DateTimeOffset from,
        DateTimeOffset to,
        string element,
        string? timeResolution,
        CancellationToken ct)
    {
        // Frost vil ha isoformat, og det er safe å bruke "O"
        var fromStr = from.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        var toStr = to.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

        var url = new StringBuilder("https://frost.met.no/observations/v0.jsonld?");
        url.Append($"sources={Uri.EscapeDataString(sourceId)}");
        url.Append($"&referencetime={Uri.EscapeDataString(fromStr)}/{Uri.EscapeDataString(toStr)}");
        url.Append($"&elements={Uri.EscapeDataString(element)}");
        if (!string.IsNullOrWhiteSpace(timeResolution))
            url.Append($"&timeresolutions={Uri.EscapeDataString(timeResolution)}");

        using var res = await client.GetAsync(url.ToString(), ct);
        if (!res.IsSuccessStatusCode) return new();

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        if (!doc.RootElement.TryGetProperty("data", out var dataArr) || dataArr.ValueKind != JsonValueKind.Array)
            return new();

        var list = new List<FrostPoint>();

        foreach (var item in dataArr.EnumerateArray())
        {
            if (!item.TryGetProperty("referenceTime", out var rtEl)) continue;
            if (!DateTimeOffset.TryParse(rtEl.GetString(), out var rt)) continue;

            if (!item.TryGetProperty("observations", out var obsArr) || obsArr.ValueKind != JsonValueKind.Array) continue;

            foreach (var obs in obsArr.EnumerateArray())
            {
                if (!obs.TryGetProperty("value", out var vEl)) continue;

                double? value = vEl.ValueKind == JsonValueKind.Number ? vEl.GetDouble() : null;
                list.Add(new FrostPoint { ReferenceTime = rt, Value = value });
            }
        }

        return list;
    }

    private sealed class FrostPoint
    {
        public DateTimeOffset ReferenceTime { get; set; }
        public double? Value { get; set; }
    }
}

public class FrostSnowSummary
{
    public string LocationName { get; init; } = "";
    public string SourceId { get; init; } = "";

    public double? SnowDepthNowCm { get; init; }
    public double? SnowDepth48hAgoCm { get; init; }
    public double? SnowDepthDelta48hCm { get; init; }

    public double PrecipLast24hMm { get; init; }
    public double PrecipLast48hMm { get; init; }
}
