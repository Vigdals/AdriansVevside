using System.Text.Json.Serialization;

namespace Adrians.Models;

public sealed class NifsStageDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("groupName")]
    public string? GroupName { get; set; }

    [JsonPropertyName("yearStart")]
    public int? YearStart { get; set; }

    [JsonPropertyName("yearEnd")]
    public int? YearEnd { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("tournament")]
    public NifsTournamentDto? Tournament { get; set; }
}

public sealed class NifsTournamentDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}