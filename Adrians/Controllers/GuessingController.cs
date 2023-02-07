using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers
{
    public class GuessingController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {

            return View();
        }
    }
}
