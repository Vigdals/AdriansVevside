using Microsoft.Extensions.Options;
using Newtonsoft.Json;

public sealed class FootballDataOptions
{
    public string ApiUrl { get; set; } =
        "https://api.football-data.org/v4/teams/81/matches";
    public string ApiKey { get; set; } = "";
}

public class FotballDataApi
{
    private readonly HttpClient _client;
    private readonly FootballDataOptions _options;

    public FotballDataApi(HttpClient client, IOptions<FootballDataOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<List<Match>> GetUpcomingMatchesAsync()
    {
        _client.DefaultRequestHeaders.Remove("X-Auth-Token");
        _client.DefaultRequestHeaders.Add("X-Auth-Token", _options.ApiKey);

        var response = await _client.GetStringAsync($"{_options.ApiUrl}?status=SCHEDULED&limit=10");
        var matchesResponse = JsonConvert.DeserializeObject<BarcelonaModel>(response);

        var upcomingMatches = new List<Match>();

        if (matchesResponse?.Matches != null)
        {
            foreach (var match in matchesResponse.Matches)
            {
                upcomingMatches.Add(new Match
                {
                    HomeTeam = match.HomeTeam.Name,
                    AwayTeam = match.AwayTeam.Name,
                    Date = match.UtcDate,
                    Status = match.Status,
                    HomeTeamLogo = match.HomeTeam.CrestUrl,
                    AwayTeamLogo = match.AwayTeam.CrestUrl,
                    HomeTeamShortName = match.HomeTeam.TeamShortName,
                    AwayTeamShortName = match.AwayTeam.TeamShortName
                });
            }
        }

        return upcomingMatches;
    }
}