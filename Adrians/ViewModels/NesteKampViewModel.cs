namespace Adrians.ViewModels;

public sealed class NesteKampViewModel
{
    public required string Tittel { get; init; }

    public required string Heimelag { get; init; }

    public required string Bortelag { get; init; }

    public string? HeimelagLogoUrl { get; init; }

    public string? BortelagLogoUrl { get; init; }

    public required string Stadion { get; init; }

    public int? Runde { get; init; }

    public required DateTimeOffset Tidspunkt { get; init; }

    public string DatoTekst => Tidspunkt.ToLocalTime().ToString("dddd d. MMMM");

    public string KlokkeslettTekst => Tidspunkt.ToLocalTime().ToString("HH:mm");
}