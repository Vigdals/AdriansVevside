using Microsoft.AspNetCore.Mvc;
using Adrians.ViewModels;
using Microsoft.AspNetCore.Authorization;


[AutoValidateAntiforgeryToken]
public class BarcaController : Controller
{
    private readonly FotballDataApi _footballService;

    public BarcaController(FotballDataApi footballService)
    {
        _footballService = footballService;
    }

    public async Task<IActionResult> Index()
    {
        var upcomingMatches = await _footballService.GetUpcomingMatchesAsync();
        // GPTsummary disabled for now
        var gptSummary = TempData["GptSummary"] as string;

        var viewModel = new BarcaViewModel
        {
            Matches = upcomingMatches,
            GptSummary = gptSummary
        };

        return View(viewModel);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> FetchGptSummary()
    {
        return RedirectToAction("Index");
    }
}