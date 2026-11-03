using System.Globalization;

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

    private static readonly CultureInfo NorwegianNynorsk = CultureInfo.GetCultureInfo("nn-NO");

    private static readonly TimeZoneInfo OsloTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Oslo");

    public string DatoTekst => TimeZoneInfo.ConvertTime(Tidspunkt, OsloTimeZone).ToString("dddd d. MMMM", NorwegianNynorsk);

    public string KlokkeslettTekst => TimeZoneInfo.ConvertTime(Tidspunkt, OsloTimeZone).ToString("HH:mm", NorwegianNynorsk);
}