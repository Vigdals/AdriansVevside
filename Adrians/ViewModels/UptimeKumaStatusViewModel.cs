namespace Adrians.ViewModels;

public enum UptimeKumaMonitorState
{
    Unknown = -1,
    Down = 0,
    Up = 1,
    Pending = 2,
    Maintenance = 3
}

public sealed class UptimeKumaStatusViewModel
{
    public IReadOnlyList<UptimeKumaMonitorViewModel> Monitors { get; init; } =
        [];

    public int TotalCount =>
        Monitors.Count;

    public int UpCount =>
        Monitors.Count(
            monitor =>
                monitor.Status ==
                UptimeKumaMonitorState.Up);

    public int DownCount =>
        Monitors.Count(
            monitor =>
                monitor.Status ==
                UptimeKumaMonitorState.Down);

    public int PendingCount =>
        Monitors.Count(
            monitor =>
                monitor.Status ==
                UptimeKumaMonitorState.Pending);

    public int MaintenanceCount =>
        Monitors.Count(
            monitor =>
                monitor.Status ==
                UptimeKumaMonitorState.Maintenance);

    public bool AllUp =>
        TotalCount > 0 &&
        UpCount == TotalCount;

    public string SummaryText
    {
        get
        {
            if (TotalCount == 0)
            {
                return "Ingen monitorar";
            }

            if (DownCount > 0)
            {
                return
                    $"{DownCount} nede · " +
                    $"{UpCount}/{TotalCount} oppe";
            }

            if (PendingCount > 0)
            {
                return
                    $"{PendingCount} ventar · " +
                    $"{UpCount}/{TotalCount} oppe";
            }

            if (MaintenanceCount > 0)
            {
                return
                    $"{MaintenanceCount} i vedlikehald · " +
                    $"{UpCount}/{TotalCount} oppe";
            }

            return
                $"{UpCount}/{TotalCount} oppe";
        }
    }
}

public sealed class UptimeKumaMonitorViewModel
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public string GroupName { get; init; } =
        string.Empty;

    public UptimeKumaMonitorState Status { get; init; } =
        UptimeKumaMonitorState.Unknown;

    public int? PingMilliseconds { get; init; }

    public double? Uptime24HoursPercent { get; init; }

    public string StatusText =>
        Status switch
        {
            UptimeKumaMonitorState.Up =>
                "Oppe",

            UptimeKumaMonitorState.Down =>
                "Nede",

            UptimeKumaMonitorState.Pending =>
                "Ventar",

            UptimeKumaMonitorState.Maintenance =>
                "Vedlikehald",

            _ =>
                "Ukjend"
        };
}