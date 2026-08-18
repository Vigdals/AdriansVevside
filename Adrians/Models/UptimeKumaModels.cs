using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adrians.Models;

internal sealed class UptimeKumaStatusPageResponse
{
    [JsonPropertyName("publicGroupList")]
    public List<UptimeKumaPublicGroupDto> PublicGroupList { get; init; } =
        [];
}

internal sealed class UptimeKumaPublicGroupDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } =
        string.Empty;

    [JsonPropertyName("monitorList")]
    public List<UptimeKumaMonitorDto> MonitorList { get; init; } =
        [];
}

internal sealed class UptimeKumaMonitorDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } =
        string.Empty;
}

internal sealed class UptimeKumaHeartbeatResponse
{
    [JsonPropertyName("heartbeatList")]
    public Dictionary<string, List<UptimeKumaHeartbeatDto>> HeartbeatList
    {
        get;
        init;
    } = new(StringComparer.Ordinal);

    [JsonPropertyName("uptimeList")]
    public Dictionary<string, JsonElement> UptimeList
    {
        get;
        init;
    } = new(StringComparer.Ordinal);
}

internal sealed class UptimeKumaHeartbeatDto
{
    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("time")]
    public string Time { get; init; } =
        string.Empty;

    [JsonPropertyName("msg")]
    public string? Message { get; init; }

    [JsonPropertyName("ping")]
    public int? PingMilliseconds { get; init; }
}