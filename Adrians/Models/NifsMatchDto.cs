using System.Text.Json.Serialization;

namespace Adrians.Models;

public sealed class NifsMatchDto
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("homeTeam")]
    public NifsTeamDto? HomeTeam { get; set; }

    [JsonPropertyName("awayTeam")]
    public NifsTeamDto? AwayTeam { get; set; }

    [JsonPropertyName("stadium")]
    public NifsStadiumDto? Stadium { get; set; }

    [JsonPropertyName("round")]
    public int? Round { get; set; }

    [JsonPropertyName("result")]
    public NifsResultDto? Result { get; set; }
}

public sealed class NifsTeamDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("logo")]
    public NifsLogoDto? Logo { get; set; }
}

public sealed class NifsLogoDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public sealed class NifsStadiumDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public sealed class NifsResultDto
{
    [JsonPropertyName("homeScore90")]
    public int? HomeScore90 { get; set; }

    [JsonPropertyName("awayScore90")]
    public int? AwayScore90 { get; set; }
}