using Adrians.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Adrians.Controllers;

public class LangrennController : Controller
{
    private readonly MeteorologiskInstituttKorttidsvarselService _korttidsvarsel;
    private readonly FrostService _frost;

    public LangrennController(MeteorologiskInstituttKorttidsvarselService korttidsvarsel, FrostService frost)
    {
        _korttidsvarsel = korttidsvarsel;
        _frost = frost;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Heggis()
    {
        ViewData["Korttidsvarsel"] =
            await _korttidsvarsel.HentKorttidsvarselAsync(
                "Heggmyrane",
                61.335538222693714,
                7.21928243332502);

        ViewData["Snow"] =
            await _frost.GetCurrentSnowDepthAsync(
                "Hafslo",
                "SN55550:0");

        return View();
    }

    public async Task<IActionResult> Hodlekve()
    {
        ViewData["Korttidsvarsel"] =
            await _korttidsvarsel.HentKorttidsvarselAsync(
                "Hodlekve",
                61.2850818,
                6.9782066);

        ViewData["Snow"] =
            await _frost.GetCurrentSnowDepthAsync(
                "Hodlekve",
                "SN55740:0");

        return View();
    }

    public async Task<IActionResult> Brunestegen()
    {
        ViewData["Korttidsvarsel"] =
            await _korttidsvarsel.HentKorttidsvarselAsync(
                "Brunestegen",
                61.41811,
                7.29553);

        return View();
    }

    public string DagerTilBirken()
    {
        var birkenStartDato = new DateTime(2023, 03, 17, 8, 0, 0);
        var daysToBirken = (birkenStartDato - DateTime.Now).TotalDays.ToString().Substring(0, 2);

        return daysToBirken;
    }
}