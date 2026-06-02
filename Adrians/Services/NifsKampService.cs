using System.Net.Http.Json;
using Adrians.Models;
using Adrians.ViewModels;

namespace Adrians.Services;

public sealed class NifsKampService
{
    private const int SogndalTeamId = 10;
    private const int Sogndal1DivisjonTournamentId = 6;
    private const int SogndalFallbackStageId = 700912;

    private const int BarcelonaTeamId = 844;
    private const int BarcelonaPrimeraDivisionTournamentId = 45;
    private const int BarcelonaFallbackStageId = 699897;

    private readonly HttpClient _httpClient;
    private readonly ILogger<NifsKampService> _logger;

    public NifsKampService(HttpClient httpClient, ILogger<NifsKampService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<NesteKampViewModel?> HentNesteSogndalKampAsync()
    {
        return HentNesteKampAsync(
            tittel: "Neste Sogndal-kamp",
            teamId: SogndalTeamId,
            tournamentId: Sogndal1DivisjonTournamentId,
            fallbackStageId: SogndalFallbackStageId);
    }

    public Task<NesteKampViewModel?> HentNesteBarcelonaKampAsync()
    {
        return HentNesteKampAsync(
            tittel: "Neste Barça-kamp",
            teamId: BarcelonaTeamId,
            tournamentId: BarcelonaPrimeraDivisionTournamentId,
            fallbackStageId: BarcelonaFallbackStageId);
    }

    private async Task<NesteKampViewModel?> HentNesteKampAsync(
        string tittel,
        int teamId,
        int tournamentId,
        int fallbackStageId)
    {
        try
        {
            var stageId = await FinnStageIdAsync(
                teamId,
                tournamentId,
                fallbackStageId);

            var kampar = await _httpClient.GetFromJsonAsync<List<NifsMatchDto>>(
                $"stages/{stageId}/matches/?teamId={teamId}");

            if (kampar is null || kampar.Count == 0)
            {
                _logger.LogInformation(
                    "NIFS returnerte ingen kampar for teamId {TeamId}, tournamentId {TournamentId}, stageId {StageId}.",
                    teamId,
                    tournamentId,
                    stageId);

                return null;
            }

            var no = DateTimeOffset.Now;

            var nesteKamp = kampar
                .Where(kamp => kamp.Timestamp > no)
                .OrderBy(kamp => kamp.Timestamp)
                .FirstOrDefault();

            if (nesteKamp is null)
            {
                _logger.LogInformation(
                    "Fann ingen framtidige kampar for teamId {TeamId}, tournamentId {TournamentId}, stageId {StageId}.",
                    teamId,
                    tournamentId,
                    stageId);

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
                "Klarte ikkje hente neste kamp frå NIFS for teamId {TeamId}, tournamentId {TournamentId}.",
                teamId,
                tournamentId);

            return null;
        }
    }

    private async Task<int> FinnStageIdAsync(
        int teamId,
        int tournamentId,
        int fallbackStageId)
    {
        try
        {
            var stages = await _httpClient.GetFromJsonAsync<List<NifsStageDto>>(
                $"teams/{teamId}/stages/?active=1");

            if (stages is null || stages.Count == 0)
            {
                _logger.LogWarning(
                    "Fann ingen stages for teamId {TeamId}. Brukar fallback stageId {FallbackStageId}.",
                    teamId,
                    fallbackStageId);

                return fallbackStageId;
            }

            var stage = stages
                .Where(stage => stage.Tournament?.Id == tournamentId)
                .OrderByDescending(stage => stage.Active == true)
                .ThenByDescending(stage => stage.YearStart ?? 0)
                .ThenByDescending(stage => stage.YearEnd ?? 0)
                .ThenByDescending(stage => stage.Id)
                .FirstOrDefault();

            if (stage is null)
            {
                _logger.LogWarning(
                    "Fann ikkje stage for teamId {TeamId}, tournamentId {TournamentId}. Brukar fallback stageId {FallbackStageId}.",
                    teamId,
                    tournamentId,
                    fallbackStageId);

                return fallbackStageId;
            }

            _logger.LogInformation(
                "Brukar NIFS stageId {StageId} ({StageName}) for teamId {TeamId}, tournamentId {TournamentId}.",
                stage.Id,
                stage.FullName ?? stage.Name,
                teamId,
                tournamentId);

            return stage.Id;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Klarte ikkje slå opp stage for teamId {TeamId}, tournamentId {TournamentId}. Brukar fallback stageId {FallbackStageId}.",
                teamId,
                tournamentId,
                fallbackStageId);

            return fallbackStageId;
        }
    }

    private static string VelVisningsnamn(NifsTeamDto? team)
    {
        return team?.ShortName
            ?? team?.Name
            ?? "Ukjent lag";
    }
}