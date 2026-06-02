using System.Net.Http.Json;
using Adrians.Models;
using Adrians.ViewModels;

namespace Adrians.Services;

public sealed class NifsKampService
{
    private const int SogndalTeamId = 10;
    private const int BarcelonaTeamId = 844;

    private readonly HttpClient _httpClient;
    private readonly ILogger<NifsKampService> _logger;

    public NifsKampService(HttpClient httpClient, ILogger<NifsKampService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<NesteKampViewModel?> HentNesteSogndalKampAsync()
    {
        return HentNesteKampForLagAsync(
            tittel: "Neste Sogndal-kamp",
            teamId: SogndalTeamId);
    }

    public Task<NesteKampViewModel?> HentNesteBarcelonaKampAsync()
    {
        return HentNesteKampForLagAsync(
            tittel: "Neste Barça-kamp",
            teamId: BarcelonaTeamId);
    }

    private async Task<NesteKampViewModel?> HentNesteKampForLagAsync(
        string tittel,
        int teamId)
    {
        try
        {
            var stages = await _httpClient.GetFromJsonAsync<List<NifsStageDto>>(
                $"teams/{teamId}/stages/?active=1");

            if (stages is null || stages.Count == 0)
            {
                _logger.LogWarning(
                    "Fann ingen aktive stages for teamId {TeamId}.",
                    teamId);

                return null;
            }

            var relevanteStages = stages
                .Where(ErRelevantStage)
                .OrderByDescending(stage => stage.Active == true)
                .ThenByDescending(stage => stage.YearStart ?? 0)
                .ThenByDescending(stage => stage.YearEnd ?? 0)
                .ThenByDescending(stage => stage.Id)
                .ToList();

            if (relevanteStages.Count == 0)
            {
                _logger.LogWarning(
                    "Fann ingen relevante stages for teamId {TeamId}.",
                    teamId);

                return null;
            }

            var alleKampar = new List<NifsMatchDto>();

            foreach (var stage in relevanteStages)
            {
                try
                {
                    var kampar = await _httpClient.GetFromJsonAsync<List<NifsMatchDto>>(
                        $"stages/{stage.Id}/matches/?teamId={teamId}");

                    if (kampar is not null && kampar.Count > 0)
                    {
                        alleKampar.AddRange(kampar);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Klarte ikkje hente kampar frå NIFS for teamId {TeamId}, stageId {StageId}.",
                        teamId,
                        stage.Id);
                }
            }

            if (alleKampar.Count == 0)
            {
                _logger.LogInformation(
                    "Fann ingen kampar i relevante stages for teamId {TeamId}.",
                    teamId);

                return null;
            }

            var no = DateTimeOffset.Now;

            var nesteKamp = alleKampar
                .Where(kamp => kamp.Timestamp > no)
                .OrderBy(kamp => kamp.Timestamp)
                .FirstOrDefault();

            if (nesteKamp is null)
            {
                _logger.LogInformation(
                    "Fann ingen framtidige kampar for teamId {TeamId}.",
                    teamId);

                return null;
            }

            return new NesteKampViewModel
            {
                Tittel = tittel,
                Heimelag = VelVisningsnamn(nesteKamp.HomeTeam),
                Bortelag = VelVisningsnamn(nesteKamp.AwayTeam),
                HeimelagLogoUrl = nesteKamp.HomeTeam?.Logo?.Url,
                BortelagLogoUrl = nesteKamp.AwayTeam?.Logo?.Url,
                Stadion = nesteKamp.Stadium?.Name ?? "Ukjent stadion",
                Runde = nesteKamp.Round,
                Tidspunkt = nesteKamp.Timestamp
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Klarte ikkje hente neste kamp frå NIFS for teamId {TeamId}.",
                teamId);

            return null;
        }
    }

    private static bool ErRelevantStage(NifsStageDto stage)
    {
        var tournament = stage.Tournament;

        if (tournament is null)
        {
            return false;
        }

        // Ikkje vis treningskampar på hovuddashboardet.
        if (string.Equals(tournament.Name, "Treningskamper", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static string VelVisningsnamn(NifsTeamDto? team)
    {
        return team?.ShortName
            ?? team?.Name
            ?? "Ukjent lag";
    }
}