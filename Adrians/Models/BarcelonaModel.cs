using Newtonsoft.Json;

namespace Adrians.Models;

public class BarcelonaModel
{
    [JsonProperty("matches")] public List<MatchDetail> Matches { get; set; }
}

public class MatchDetail
{
    [JsonProperty("homeTeam")] public Team HomeTeam { get; set; }

    [JsonProperty("awayTeam")] public Team AwayTeam { get; set; }

    [JsonProperty("utcDate")] public string UtcDate { get; set; }

    [JsonProperty("status")] public string Status { get; set; }
}

public class Team
{
    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("crest")] public string CrestUrl { get; set; }

    [JsonProperty("shortName")] public string TeamShortName { get; set; }
}

public class Match
{
    public string HomeTeam { get; set; }
    public string AwayTeam { get; set; }
    public string Date { get; set; }
    public string Status { get; set; }
    public string HomeTeamLogo { get; set; }
    public string AwayTeamLogo { get; set; }
    public string HomeTeamShortName { get; set; }
    public string AwayTeamShortName { get; set; }
}