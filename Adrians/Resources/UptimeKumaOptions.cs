namespace Adrians.Resources;

public sealed class UptimeKumaOptions
{
    public const string SectionName =
        "UptimeKuma";

    public bool Enabled { get; init; }

    public string BaseUrl { get; init; } =
        string.Empty;

    public string StatusPageSlug { get; init; } =
        string.Empty;

    public int CacheSeconds { get; init; } =
        60;
}