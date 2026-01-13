namespace Adrians.ViewModels
{
    public class KorttidsvarselViewModel
    {
        public string LocationName { get; init; } = "";
        public DateTimeOffset UpdatedAt { get; init; }

        public double? TemperatureC { get; init; }
        public double? WindSpeedMs { get; init; }
        public double? WindGustMs { get; init; }
        public double? PrecipNext1hMm { get; init; }

        public string? SymbolCode { get; init; } // optional
    }
}
