namespace Adrians.ViewModels;

public sealed class PublicDashboardViewModel
{
    public required string Stadnamn { get; init; }

    public KorttidsvarselViewModel? Korttidsvarsel { get; init; }

    public NesteSogndalKampViewModel? NesteSogndalKamp { get; init; }

    public required DateTimeOffset SistOppdatert { get; init; }

    public required IReadOnlyList<DashboardCountdownViewModel> Countdowns { get; init; }

    public required IReadOnlyList<DashboardInfoCardViewModel> InfoCards { get; init; }

    public required IReadOnlyList<DashboardLinkViewModel> Links { get; init; }
}

public sealed class DashboardCountdownViewModel
{
    public required string Id { get; init; }

    public required string Tittel { get; init; }

    public required string Undertittel { get; init; }

    public required DateTimeOffset Tidspunkt { get; init; }

    public string? BildeUrl { get; init; }

    public string? AltTekst { get; init; }
}

public sealed class DashboardInfoCardViewModel
{
    public required string Tittel { get; init; }

    public required string Verdi { get; init; }

    public required string Tekst { get; init; }

    public string? Ikon { get; init; }
}

public sealed class DashboardLinkViewModel
{
    public required string Tittel { get; init; }

    public required string Url { get; init; }

    public string? Tekst { get; init; }
}