using Adrians.ViewModels;
using Microsoft.AspNetCore.Mvc;

public sealed class BarcaController(FotballDataApi footballService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var upcomingMatches = await footballService.GetUpcomingMatchesAsync();
        return View(new BarcaViewModel { Matches = upcomingMatches });
    }
}
