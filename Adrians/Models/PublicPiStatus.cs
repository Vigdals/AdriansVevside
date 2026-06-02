namespace Adrians.Models;

public sealed class PublicPiStatus
{
    public string Status { get; set; } = "unknown";
    public string Commit { get; set; } = "";
    public string CommitShort => string.IsNullOrWhiteSpace(Commit)
        ? ""
        : Commit[..Math.Min(7, Commit.Length)];

    public DateTimeOffset? DeployedAt { get; set; }
    public DateTimeOffset? CheckedAt { get; set; }

    public string DotnetRuntime { get; set; } = "";
    public string AppEnvironment { get; set; } = "";
    public string HealthUrl { get; set; } = "";

    public long? ResponseMs { get; set; }
    public string Message { get; set; } = "";

    public bool IsOk => string.Equals(Status, "ok", StringComparison.OrdinalIgnoreCase);
}