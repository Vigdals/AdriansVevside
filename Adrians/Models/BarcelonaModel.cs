using Newtonsoft.Json;

namespace Adrians.Models;

public class BarcelonaModel
{
    [JsonProperty("matches")] public List<MatchDetail> Matches { get; set; } = [];
}

public class MatchDetail
{
    [JsonProperty("homeTeam")] public Team HomeTeam { get; set; } = new();

    [JsonProperty("awayTeam")] public Team AwayTeam { get; set; } = new();

    [JsonProperty("utcDate")] public string UtcDate { get; set; } = string.Empty;

    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
}

public class Team
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;

    [JsonProperty("crest")] public string CrestUrl { get; set; } = string.Empty;

    [JsonProperty("shortName")] public string TeamShortName { get; set; } = string.Empty;
}

public class Match
{
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string HomeTeamLogo { get; set; } = string.Empty;
    public string AwayTeamLogo { get; set; } = string.Empty;
    public string HomeTeamShortName { get; set; } = string.Empty;
    public string AwayTeamShortName { get; set; } = string.Empty;
}