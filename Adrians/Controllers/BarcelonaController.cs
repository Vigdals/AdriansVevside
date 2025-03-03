using Adrians.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Adrians.Models;

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
