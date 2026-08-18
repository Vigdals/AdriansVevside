using System.Globalization;
using System.Text.Json;
using Adrians.Models;
using Adrians.Resources;
using Adrians.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Adrians.Services;

public sealed class UptimeKumaService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    private readonly HttpClient _httpClient;
    private readonly UptimeKumaOptions _options;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<UptimeKumaService> _logger;

    public UptimeKumaService(
        HttpClient httpClient,
        IOptions<UptimeKumaOptions> options,
        IMemoryCache memoryCache,
        ILogger<UptimeKumaService> logger)
    {
        _httpClient =
            httpClient;

        _options =
            options.Value;

        _memoryCache =
            memoryCache;

        _logger =
            logger;
    }

    public async Task<UptimeKumaStatusViewModel?> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        var cacheKey =
            $"UptimeKuma:{_options.StatusPageSlug}";

        if (_memoryCache.TryGetValue<UptimeKumaStatusViewModel>(
                cacheKey,
                out var cachedStatus) &&
            cachedStatus is not null)
        {
            return cachedStatus;
        }

        var slug =
            Uri.EscapeDataString(
                _options.StatusPageSlug);

        _logger.LogInformation(
            "Hentar status frå Uptime Kuma for status-side {Slug}.",
            _options.StatusPageSlug);

        var statusPage =
            await GetJsonAsync<UptimeKumaStatusPageResponse>(
                $"api/status-page/{slug}",
                cancellationToken);

        var heartbeatStatus =
            await GetJsonAsync<UptimeKumaHeartbeatResponse>(
                $"api/status-page/heartbeat/{slug}",
                cancellationToken);

        var monitors =
            statusPage.PublicGroupList
                .SelectMany(
                    group =>
                        group.MonitorList.Select(
                            monitor =>
                                CreateMonitorViewModel(
                                    group,
                                    monitor,
                                    heartbeatStatus)))
                .GroupBy(
                    monitor =>
                        monitor.Id)
                .Select(
                    group =>
                        group.First())
                .ToList();

        var result =
            new UptimeKumaStatusViewModel
            {
                Monitors =
                    monitors
            };

        _memoryCache.Set(
            cacheKey,
            result,
            TimeSpan.FromSeconds(
                _options.CacheSeconds));

        _logger.LogInformation(
            "Henta Uptime Kuma-status. {UpCount}/{TotalCount} er oppe.",
            result.UpCount,
            result.TotalCount);

        return result;
    }

    private UptimeKumaMonitorViewModel CreateMonitorViewModel(
        UptimeKumaPublicGroupDto group,
        UptimeKumaMonitorDto monitor,
        UptimeKumaHeartbeatResponse heartbeatStatus)
    {
        var monitorKey =
            monitor.Id.ToString(
                CultureInfo.InvariantCulture);

        UptimeKumaHeartbeatDto? latestHeartbeat =
            null;

        if (heartbeatStatus.HeartbeatList.TryGetValue(
                monitorKey,
                out var heartbeats) &&
            heartbeats.Count > 0)
        {
            /*
             * Uptime Kuma brukar eit kronologisk,
             * ISO-liknande tidsformat.
             *
             * Sortering på tidsstrengen gjer oss uavhengige
             * av om API-et returnerer eldste eller nyaste først.
             */
            latestHeartbeat =
                heartbeats
                    .OrderByDescending(
                        heartbeat =>
                            heartbeat.Time,
                        StringComparer.Ordinal)
                    .FirstOrDefault();
        }

        return new UptimeKumaMonitorViewModel
        {
            Id =
                monitor.Id,

            Name =
                monitor.Name,

            GroupName =
                group.Name,

            Status =
                MapStatus(
                    latestHeartbeat?.Status),

            PingMilliseconds =
                latestHeartbeat?
                    .PingMilliseconds,

            Uptime24HoursPercent =
                Get24HourUptime(
                    heartbeatStatus,
                    monitor.Id)
        };
    }

    private static UptimeKumaMonitorState MapStatus(
        int? status)
    {
        return status switch
        {
            0 =>
                UptimeKumaMonitorState.Down,

            1 =>
                UptimeKumaMonitorState.Up,

            2 =>
                UptimeKumaMonitorState.Pending,

            3 =>
                UptimeKumaMonitorState.Maintenance,

            _ =>
                UptimeKumaMonitorState.Unknown
        };
    }

    private static double? Get24HourUptime(
        UptimeKumaHeartbeatResponse heartbeatStatus,
        int monitorId)
    {
        var key =
            $"{monitorId}_24";

        if (!heartbeatStatus.UptimeList.TryGetValue(
                key,
                out var value))
        {
            return null;
        }

        double? rawValue =
            null;

        if (value.ValueKind == JsonValueKind.Number &&
            value.TryGetDouble(out var numericValue))
        {
            rawValue =
                numericValue;
        }
        else if (value.ValueKind == JsonValueKind.String &&
                 double.TryParse(
                     value.GetString(),
                     NumberStyles.Float,
                     CultureInfo.InvariantCulture,
                     out numericValue))
        {
            rawValue =
                numericValue;
        }

        if (!rawValue.HasValue ||
            !double.IsFinite(rawValue.Value))
        {
            return null;
        }

        /*
         * Kuma returnerer normalt uptime som 0..1.
         * Dersom formatet ein dag kjem som 0..100,
         * toler me det òg.
         */
        if (rawValue.Value >= 0 &&
            rawValue.Value <= 1)
        {
            return
                rawValue.Value * 100;
        }

        if (rawValue.Value > 1 &&
            rawValue.Value <= 100)
        {
            return
                rawValue.Value;
        }

        return null;
    }

    private async Task<T> GetJsonAsync<T>(
        string requestUri,
        CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.GetAsync(
                requestUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Uptime Kuma returnerte HTTP " +
                $"{(int)response.StatusCode} for {requestUri}.",
                inner: null,
                response.StatusCode);
        }

        await using var stream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        var result =
            await JsonSerializer.DeserializeAsync<T>(
                stream,
                JsonOptions,
                cancellationToken);

        return result
            ?? throw new JsonException(
                "Uptime Kuma returnerte eit tomt " +
                "eller ugyldig JSON-svar.");
    }
}