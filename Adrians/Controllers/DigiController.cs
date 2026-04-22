// Controllers/DigiController.cs
using Adrians.Services;
using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers;

public class DigiController(RssFeedService rss) : Controller
{
    private const string DigiUrl = "https://www.digi.no/feeds/general.xml";
    //private const string Kode24Url = "https://www.kode24.no/rss";

    public async Task<IActionResult> Index()
    {
        var items = await rss.GetItemsAsync(DigiUrl);
        return View(items);
    }
}