namespace Adrians.Helpers
{
    public class MetIconHelper
    {
        public static string GetIconFile(string? symbolCode)
        {
            if (string.IsNullOrWhiteSpace(symbolCode)) return "unknown";

            // sjekkar symbolCode er trygg
            var safe = symbolCode
                .Replace("..", "")
                .Replace("/", "")
                .Replace("\\", "")
                .Trim();

            return safe;
        }
    }
}
