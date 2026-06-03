using System.Net.Http.Headers;
using System.Text.Json;
using Adrians.Models;
using Adrians.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Adrians.Services;

public sealed class SimasTommekalenderService
{
    private const string CacheKey = "SimasTommekalender:Leitevegen15";

    private const string Adresse = "Leitevegen 15";
    private const string Kommunenr = "4640";
    private const string Gatenavn = "Leitevegen";
    private const string Gatekode = "464003200";
    private const string Husnr = "15";

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SimasTommekalenderService> _logger;

    public SimasTommekalenderService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<SimasTommekalenderService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TommekalenderViewModel> HentTommekalenderAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out TommekalenderViewModel? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var apiItems = await HentFraApiAsync(cancellationToken);

            var viewModel = new TommekalenderViewModel
            {
                Adresse = Adresse,
                SistOppdatert = DateTimeOffset.Now,
                Tommingar = apiItems
                    .Select(MapToViewModel)
                    .OrderBy(x => x.NesteTomming ?? DateOnly.MaxValue)
                    .ThenBy(x => x.FraksjonId)
                    .ToList()
            };

            _cache.Set(
                CacheKey,
                viewModel,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12),
                    SlidingExpiration = TimeSpan.FromHours(2)
                });

            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Klarte ikkje hente tømmekalender frå SIMAS/Norkart.");

            return new TommekalenderViewModel
            {
                Adresse = Adresse,
                Feilmelding = "Tømmekalender kunne ikkje hentast akkurat no."
            };
        }
    }

    private async Task<IReadOnlyList<SimasTommekalenderItem>> HentFraApiAsync(
        CancellationToken cancellationToken)
    {
        var innerUrl =
            "https://komteksky.norkart.no/MinRenovasjon.Api/api/tommekalender/?" +
            $"gatenavn={Uri.EscapeDataString(Gatenavn + " ")}" +
            $"&gatekode={Uri.EscapeDataString(Gatekode)}" +
            $"&husnr={Uri.EscapeDataString(Husnr)}";

        var url =
            "https://norkartrenovasjon.azurewebsites.net/proxyserver.ashx?server=" +
            Uri.EscapeDataString(innerUrl);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.TryAddWithoutValidation("Kommunenr", Kommunenr);
        request.Headers.TryAddWithoutValidation("Origin", "https://www.simas.no");
        request.Headers.TryAddWithoutValidation("Referer", "https://www.simas.no/");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "SIMAS-kall feila. Status={StatusCode}. Body={Body}",
                response.StatusCode,
                body);

            return [];
        }

        return JsonSerializer.Deserialize<List<SimasTommekalenderItem>>(
            body,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];
    }

    private static TommingViewModel MapToViewModel(SimasTommekalenderItem item)
    {
        var datoar = item.Tommedatoer
            .OrderBy(x => x)
            .Select(DateOnly.FromDateTime)
            .ToList();

        var metadata = GetFraksjonMetadata(item.FraksjonId);

        return new TommingViewModel
        {
            FraksjonId = item.FraksjonId,
            Namn = metadata.Namn,
            CssClass = metadata.CssClass,
            Ikon = metadata.Ikon,
            NesteTomming = datoar.ElementAtOrDefault(0),
            TommingDeretter = datoar.ElementAtOrDefault(1)
        };
    }

    private static FraksjonMetadata GetFraksjonMetadata(int fraksjonId)
    {
        return fraksjonId switch
        {
            1 => new FraksjonMetadata(
                "Restavfall",
                "dashboard-waste-rest",
                "🗑️"),

            2 => new FraksjonMetadata(
                "Papir",
                "dashboard-waste-paper",
                "📄"),

            3 => new FraksjonMetadata(
                "Våtorganisk",
                "dashboard-waste-organic",
                "♻️"),

            4 => new FraksjonMetadata(
                "Glas og metall",
                "dashboard-waste-glass-metal",
                "🍾"),

            7 => new FraksjonMetadata(
                "Plast",
                "dashboard-waste-plastic",
                "🧴"),

            _ => new FraksjonMetadata(
                $"Ukjend fraksjon {fraksjonId}",
                "dashboard-waste-unknown",
                "❔")
        };
    }

    private sealed record FraksjonMetadata(
        string Namn,
        string CssClass,
        string Ikon);
}