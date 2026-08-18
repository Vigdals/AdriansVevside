using Adrians.Models;

namespace Adrians.ViewModels;

public enum VigdalPiOverallState
{
    Unknown,
    Healthy,
    Warning,
    Critical
}

public sealed class VigdalPiStatusViewModel
{
    public PublicPiStatus? PiStatus { get; init; }

    public UptimeKumaStatusViewModel? UptimeKumaStatus { get; init; }

    public bool HasServiceStatus =>
        UptimeKumaStatus is
        {
            TotalCount: > 0
        };

    public bool HasHostMetrics =>
        PiStatus is not null &&
        (
            !string.IsNullOrWhiteSpace(
                PiStatus.Temperature) ||

            !string.IsNullOrWhiteSpace(
                PiStatus.HostUptime) ||

            !string.IsNullOrWhiteSpace(
                PiStatus.LoadAverage) ||

            !string.IsNullOrWhiteSpace(
                PiStatus.DiskUsage)
        );

    public bool DeployStatusKnown =>
        PiStatus is not null &&
        !string.Equals(
            PiStatus.Status,
            "unknown",
            StringComparison.OrdinalIgnoreCase);

    public VigdalPiOverallState OverallState
    {
        get
        {
            if (UptimeKumaStatus?.DownCount > 0)
            {
                return VigdalPiOverallState.Critical;
            }

            if (UptimeKumaStatus?.AllUp == true &&
                PiStatus?.IsOk == true)
            {
                return VigdalPiOverallState.Healthy;
            }

            if (HasServiceStatus ||
                DeployStatusKnown)
            {
                return VigdalPiOverallState.Warning;
            }

            return VigdalPiOverallState.Unknown;
        }
    }

    public string OverallStatusText
    {
        get
        {
            if (UptimeKumaStatus?.DownCount > 0)
            {
                return UptimeKumaStatus.DownCount == 1
                    ? "1 teneste nede"
                    : $"{UptimeKumaStatus.DownCount} tenester nede";
            }

            if (UptimeKumaStatus?.AllUp == true &&
                PiStatus?.IsOk == true)
            {
                return "Alt OK";
            }

            if (UptimeKumaStatus?.AllUp == true)
            {
                return PiStatus?.Status
                    .ToLowerInvariant() switch
                {
                    "failed" =>
                        "Tenestene oppe · deploy feila",

                    "deploying" =>
                        "Tenestene oppe · deploy pågår",

                    _ =>
                        "Tenestene oppe"
                };
            }

            if (UptimeKumaStatus?.MaintenanceCount > 0)
            {
                return "Vedlikehald";
            }

            if (UptimeKumaStatus?.PendingCount > 0)
            {
                return "Status blir sjekka";
            }

            if (PiStatus?.IsOk == true)
            {
                return "Deploy OK";
            }

            return "Status ukjend";
        }
    }

    public string DeployStatusText =>
        PiStatus?.Status
            .ToLowerInvariant() switch
        {
            "ok" =>
                "Siste deploy var OK",

            "deploying" =>
                "Deploy pågår",

            "failed" =>
                "Siste deploy feila",

            _ =>
                "Deploy-status er uklar"
        };

    public string ServiceSummary =>
        UptimeKumaStatus?.SummaryText
        ?? "Tenestestatus ukjend";
}