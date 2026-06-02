using System.Diagnostics;
using Adrians.Models;
using Adrians.Services;
using Adrians.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers;

public class HomeController : Controller
{
    private const double SogndalLat = 61.22908;
    private const double SogndalLon = 7.09674;

    private readonly ILogger<HomeController> _logger;
    private readonly MeteorologiskInstituttKorttidsvarselService _korttidsvarsel;
    private readonly NifsKampService _nifsKampService;
    private readonly PublicPiStatusService _piStatusService;

    public HomeController(
        ILogger<HomeController> logger,
        MeteorologiskInstituttKorttidsvarselService korttidsvarsel,
        NifsKampService nifsKampService,
        PublicPiStatusService piStatusService)
    {
        _logger = logger;
        _korttidsvarsel = korttidsvarsel;
        _nifsKampService = nifsKampService;
        _piStatusService = piStatusService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await BuildPublicDashboardAsync();
        return View(model);
    }

    private async Task<PublicDashboardViewModel> BuildPublicDashboardAsync()
    {
        KorttidsvarselViewModel? varsel = null;
        NesteKampViewModel? nesteSogndalKamp = null;
        NesteKampViewModel? nesteBarcelonaKamp = null;
        PublicPiStatus? piStatus = null;

        try
        {
            varsel = await _korttidsvarsel.HentKorttidsvarselAsync(
                "Sogndal",
                SogndalLat,
                SogndalLon);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Klarte ikkje hente korttidsvarsel.");
        }

        try
        {
            nesteSogndalKamp = await _nifsKampService.HentNesteSogndalKampAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Klarte ikkje hente neste Sogndal-kamp.");
        }

        try
        {
            nesteBarcelonaKamp = await _nifsKampService.HentNesteBarcelonaKampAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Klarte ikkje hente neste Barcelona-kamp.");
        }

        try
        {
            piStatus = await _piStatusService.GetStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Klarte ikkje hente public Pi-status.");

            piStatus = new PublicPiStatus
            {
                Status = "unknown",
                Message = "Pi-status kunne ikkje hentast akkurat no."
            };
        }

        return new PublicDashboardViewModel
        {
            Stadnamn = "Sogndal",
            Korttidsvarsel = varsel,
            NesteSogndalKamp = nesteSogndalKamp,
            NesteBarcelonaKamp = nesteBarcelonaKamp,
            PiStatus = piStatus,
            SistOppdatert = DateTimeOffset.Now,

            Countdowns =
            [
                new DashboardCountdownViewModel
                {
                    Id = "CountdownSTR",
                    Tittel = "Sognefjord Trail Run",
                    Undertittel = "",
                    Tidspunkt = new DateTimeOffset(2026, 6, 6, 8, 0, 0, TimeSpan.FromHours(2)),
                    BildeUrl = "/img/STR.png",
                    AltTekst = "Sognefjord Trail Run"
                },
                new DashboardCountdownViewModel
                {
                    Id = "CountdownLFI",
                    Tittel = "Lustrafjorden Inn",
                    Undertittel = "",
                    Tidspunkt = new DateTimeOffset(2026, 8, 15, 8, 0, 0, TimeSpan.FromHours(2)),
                    BildeUrl = "/img/lustrafjorden_inn.png",
                    AltTekst = "Lustrafjorden Inn"
                }
            ],

            InfoCards = [],

            Links =
            [
                new DashboardLinkViewModel
                {
                    Tittel = "Hacker News",
                    Url = "/HackerNews",
                    Tekst = "Nerdepåfyll"
                },
                new DashboardLinkViewModel
                {
                    Tittel = "Barça",
                    Url = "/Barca",
                    Tekst = "Fotball"
                },
                new DashboardLinkViewModel
                {
                    Tittel = "FPL",
                    Url = "/FPL",
                    Tekst = "Fantasy"
                }
            ]
        };
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