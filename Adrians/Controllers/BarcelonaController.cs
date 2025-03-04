using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers
{
    public class BarcelonaController : Controller
    {
        public async Task<ActionResult> IndexAsync()
        {
            var upcomingMatches = await _footballService.GetUpcomingMatchesAsync();
            return View(upcomingMatches);
        }
        private readonly FotballDataApi _footballService;

        public BarcelonaController()
        {
            _footballService = new FotballDataApi();
        }
    }
}