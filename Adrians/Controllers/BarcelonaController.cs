using Microsoft.AspNetCore.Mvc;
using Adrians.ViewModels;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[AutoValidateAntiforgeryToken]
public class BarcaController : Controller
{
    private readonly FotballDataApi _footballService;
    private readonly ChatGptService _chatGptService;

    public BarcaController()
    {
        _footballService = new FotballDataApi();
        _chatGptService = new ChatGptService();
    }

    public async Task<IActionResult> Index()
    {
        var upcomingMatches = await _footballService.GetUpcomingMatchesAsync();
        var gptSummary = TempData["GptSummary"] as string;

        var viewModel = new BarcaViewModel
        {
            Matches = upcomingMatches,
            GptSummary = gptSummary
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> FetchGptSummary()
    {
        var gptSummary = await _chatGptService.GetBarcaSummaryAsync();
        TempData["GptSummary"] = gptSummary;

        return RedirectToAction("Index");
    }
}