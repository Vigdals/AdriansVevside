using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers;

public class LangrennController : Controller
{
    public IActionResult Index()
    {
        var tidTilBirken = DagerTilBirken();
        ViewBag.tidTilBirken = tidTilBirken;
        Debug.WriteLine(tidTilBirken);
        return View();
    }

    public IActionResult Heggis()
    {
        return View();
    }

    public IActionResult Brunestegen()
    {
        return View();
    }

    public IActionResult Hodlekve()
    {
        return View();
    }

    public string DagerTilBirken()
    {
        var birkenStartDato = new DateTime(2023, 03, 17, 8, 0, 0);
        var daysToBirken = (birkenStartDato - DateTime.Now).TotalDays.ToString().Substring(0, 2);

        return daysToBirken;
    }
}