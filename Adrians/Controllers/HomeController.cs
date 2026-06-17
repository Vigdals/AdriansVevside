using System.Diagnostics;
using Adrians.Models;
using Adrians.Services;
using Adrians.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers;

public sealed class HomeController : Controller
{
    private const string Stadnamn = "Sogndal";
    private const double SogndalLat = 61.22908;
    private const double SogndalLon = 7.09674;

    private static readonly IReadOnlyList<DashboardCountdownViewModel> DashboardCountdowns =
    [
        new()
        {
            Id = "CountdownSTR",
            Tittel = "Sognefjord Trail Run",
            Undertittel = "",
            Tidspunkt = new DateTimeOffset(2026, 6, 6, 8, 0, 0, TimeSpan.FromHours(2)),
            BildeUrl = "/img/STR.png",
            AltTekst = "Sognefjord Trail Run"
        },
        new()
        {
            Id = "CountdownLFI",
            Tittel = "Lustrafjorden Inn",
            Undertittel = "",
            Tidspunkt = new DateTimeOffset(2026, 8, 15, 8, 0, 0, TimeSpan.FromHours(2)),
            BildeUrl = "/img/lustrafjorden_inn.png",
            AltTekst = "Lustrafjorden Inn"
        }
    ];

    private static readonly IReadOnlyList<DashboardInfoCardViewModel> DashboardInfoCards = [];

    private static readonly IReadOnlyList<DashboardLinkViewModel> DashboardLinks =
    [
        new()
        {
            Tittel = "Hacker News",
            Url = "/HackerNews",
            Tekst = "Nerdepåfyll"
        },
        new()
        {
            Tittel = "Barça",
            Url = "/Barca",
            Tekst = "Fotball"
        },
        new()
        {
            Tittel = "FPL",
            Url = "/FPL",
            Tekst = "Fantasy"
        }
    ];

    private readonly ILogger<HomeController> _logger;
    private readonly MeteorologiskInstituttKorttidsvarselService _korttidsvarsel;
    private readonly NifsKampService _nifsKampService;
    private readonly PublicPiStatusService _piStatusService;
    private readonly SimasTommekalenderService _tommekalenderService;

    public HomeController(
        ILogger<HomeController> logger,
        MeteorologiskInstituttKorttidsvarselService korttidsvarsel,
        NifsKampService nifsKampService,
        PublicPiStatusService piStatusService,
        SimasTommekalenderService tommekalenderService)
    {
        _logger = logger;
        _korttidsvarsel = korttidsvarsel;
        _nifsKampService = nifsKampService;
        _piStatusService = piStatusService;
        _tommekalenderService = tommekalenderService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await BuildPublicDashboardAsync(cancellationToken);
        return View(model);
    }

    private async Task<PublicDashboardViewModel> BuildPublicDashboardAsync(
        CancellationToken cancellationToken)
    {
        var korttidsvarselTask = TryLoadAsync(
            operation: ct => _korttidsvarsel.HentKorttidsvarselAsync(
                Stadnamn,
                SogndalLat,
                SogndalLon,
                ct),
            errorMessage: "Klarte ikkje hente korttidsvarsel.",
            cancellationToken: cancellationToken);

        var nesteSogndalKampTask = TryLoadAsync(
            operation: _ => _nifsKampService.HentNesteSogndalKampAsync(),
            errorMessage: "Klarte ikkje hente neste Sogndal-kamp.",
            cancellationToken: cancellationToken);

        var nesteBarcelonaKampTask = TryLoadAsync(
            operation: _ => _nifsKampService.HentNesteBarcelonaKampAsync(),
            errorMessage: "Klarte ikkje hente neste Barcelona-kamp.",
            cancellationToken: cancellationToken);

        var piStatusTask = TryLoadAsync(
            operation: ct => _piStatusService.GetStatusAsync(ct),
            errorMessage: "Klarte ikkje hente public Pi-status.",
            cancellationToken: cancellationToken,
            fallback: new PublicPiStatus
            {
                Status = "unknown",
                Message = "Pi-status kunne ikkje hentast akkurat no."
            });

        var tommekalenderTask = TryLoadAsync(
            operation: ct => _tommekalenderService.HentTommekalenderAsync(ct),
            errorMessage: "Klarte ikkje hente tømmekalender.",
            cancellationToken: cancellationToken,
            fallback: new TommekalenderViewModel
            {
                Adresse = "Leitevegen 15",
                Feilmelding = "Tømmekalender kunne ikkje hentast akkurat no."
            });

        await Task.WhenAll(
            korttidsvarselTask,
            nesteSogndalKampTask,
            nesteBarcelonaKampTask,
            piStatusTask,
            tommekalenderTask);

        return new PublicDashboardViewModel
        {
            Stadnamn = Stadnamn,
            Korttidsvarsel = await korttidsvarselTask,
            NesteSogndalKamp = await nesteSogndalKampTask,
            NesteBarcelonaKamp = await nesteBarcelonaKampTask,
            PiStatus = await piStatusTask,
            Tommekalender = await tommekalenderTask,
            SistOppdatert = DateTimeOffset.Now,
            Countdowns = DashboardCountdowns,
            InfoCards = DashboardInfoCards,
            Links = DashboardLinks
        };
    }

    private async Task<T?> TryLoadAsync<T>(
        Func<CancellationToken, Task<T?>> operation,
        string errorMessage,
        CancellationToken cancellationToken,
        T? fallback = default)
    {
        try
        {
            return await operation(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{ErrorMessage}", errorMessage);
            return fallback;
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}