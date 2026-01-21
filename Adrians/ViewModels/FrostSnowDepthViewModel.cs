namespace Adrians.ViewModels
{
    public sealed class FrostSnowDepth
    {
        public string LocationName { get; init; } = "";
        public string SourceId { get; init; } = "";
        public double? SnowDepthCm { get; init; }
        public DateTimeOffset? ObservedAt { get; init; }
    }
}