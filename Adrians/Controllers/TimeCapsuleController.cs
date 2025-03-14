using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers;

public class TimeCapsuleController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult LogHubSite()
    {
        return View("Index");
    }

    //Just a task to send log to https://loghub.statsforvalteren.no/
}