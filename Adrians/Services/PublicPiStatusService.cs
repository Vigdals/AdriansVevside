using System.Text.Json;
using Adrians.Models;

namespace Adrians.Services;

public sealed class PublicPiStatusService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PublicPiStatusService> _logger;

    public PublicPiStatusService(
        IWebHostEnvironment environment,
        ILogger<PublicPiStatusService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<PublicPiStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(
            _environment.WebRootPath,
            "status",
            "deploy.json");

        if (!File.Exists(path))
        {
            return new PublicPiStatus
            {
                Status = "unknown",
                Message = "Ingen deploy-status funnen enno."
            };
        }

        try
        {
            await using var stream = File.OpenRead(path);

            var status = await JsonSerializer.DeserializeAsync<PublicPiStatus>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken);

            if (status is null)
            {
                return new PublicPiStatus
                {
                    Status = "unknown",
                    Message = "Deploy-statusfila var tom eller ugyldig."
                };
            }

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read public deploy status file.");

            return new PublicPiStatus
            {
                Status = "unknown",
                Message = "Deploy-status er mellombels utilgjengeleg."
            };
        }
    }
}