using System.Net.Http.Json;
using Adrians.Models;
using Adrians.ViewModels;

namespace Adrians.Services;

public sealed class NifsKampService
{
    private const string NesteSogndalKampEndpoint = "stages/700912/matches/?teamId=10";

    private readonly HttpClient _httpClient;
    private readonly ILogger<NifsKampService> _logger;

    public NifsKampService(HttpClient httpClient, ILogger<NifsKampService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<NesteSogndalKampViewModel?> HentNesteSogndalKampAsync()
    {
        try
        {
            var kampar = await _httpClient.GetFromJsonAsync<List<NifsMatchDto>>(NesteSogndalKampEndpoint);

            if (kampar is null || kampar.Count == 0)
            {
                return null;
            }

            var no = DateTimeOffset.Now;

            var nesteKamp = kampar
                .Where(kamp => kamp.Timestamp > no)
                .OrderBy(kamp => kamp.Timestamp)
                .FirstOrDefault();

            if (nesteKamp is null)
            {
                return null;
            }

            return new NesteSogndalKampViewModel
            {
                Kampnamn = nesteKamp.Name ?? "Sogndal-kamp",
                Heimelag = nesteKamp.HomeTeam?.Name ?? "Ukjent heimelag",
                Bortelag = nesteKamp.AwayTeam?.Name ?? "Ukjent bortelag",
                HeimelagLogoUrl = nesteKamp.HomeTeam?.Logo?.Url,
                BortelagLogoUrl = nesteKamp.AwayTeam?.Logo?.Url,
                Stadion = nesteKamp.Stadium?.Name ?? "Ukjent stadion",
                Runde = nesteKamp.Round,
                Tidspunkt = nesteKamp.Timestamp
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Klarte ikkje hente neste Sogndal-kamp frå NIFS.");
            return null;
        }
    }
}