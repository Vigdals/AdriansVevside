using Microsoft.AspNetCore.Mvc;

namespace Adrians.Models;

public sealed class NextMatchViewModel
{
    public bool HasMatch => KickoffUtc.HasValue;
    public string? Competition { get; init; }
    public string? Opponent { get; init; }
    public bool IsHome { get; init; }
    public DateTime? KickoffUtc { get; init; }
    public string? OpponentCrest { get; init; }
    public string KickoffIso => KickoffUtc?.ToString("o") ?? "";
}
