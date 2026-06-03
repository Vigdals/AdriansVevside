namespace Adrians.ViewModels;

public sealed class TommekalenderViewModel
{
    public string Adresse { get; set; } = string.Empty;

    public IReadOnlyList<TommingViewModel> Tommingar { get; set; } = [];

    public DateTimeOffset? SistOppdatert { get; set; }

    public string? Feilmelding { get; set; }

    public bool HarData => Tommingar.Count > 0;
}

public sealed class TommingViewModel
{
    public int FraksjonId { get; set; }

    public string Namn { get; set; } = string.Empty;

    public string CssClass { get; set; } = string.Empty;

    public string Ikon { get; set; } = string.Empty;

    public DateOnly? NesteTomming { get; set; }

    public DateOnly? TommingDeretter { get; set; }
}