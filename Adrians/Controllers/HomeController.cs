using System.Diagnostics;
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

    public HomeController(
        ILogger<HomeController> logger,
        MeteorologiskInstituttKorttidsvarselService korttidsvarsel,
        NifsKampService nifsKampService)
    {
        _logger = logger;
        _korttidsvarsel = korttidsvarsel;
        _nifsKampService = nifsKampService;
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
        NesteSogndalKampViewModel? nesteSogndalKamp = null;

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

        return new PublicDashboardViewModel
        {
            Stadnamn = "Sogndal",
            Korttidsvarsel = varsel,
            NesteSogndalKamp = nesteSogndalKamp,
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

            InfoCards =
            [
                new DashboardInfoCardViewModel
                {
                    Tittel = "Pi-status",
                    Verdi = "Oppe",
                    Tekst = "Offentleg status. Detaljar kjem på privat dashboard.",
                    Ikon = "🟢"
                },
                new DashboardInfoCardViewModel
                {
                    Tittel = "Neste Barça-kamp",
                    Verdi = "Kjem",
                    Tekst = "Koplar på FootballData i neste steg.",
                    Ikon = "🔵"
                }
            ],

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