using Microsoft.AspNetCore.Mvc;

namespace Adrians.Resources;
public sealed class FootballDataOptions
{
    public string ApiKey { get; set; } = string.Empty;       // from secrets.json
    public string BaseUrl { get; set; } = "https://api.football-data.org/v4/";
    public int DefaultTeamId { get; set; } = 81;              // FC Barcelona
}
