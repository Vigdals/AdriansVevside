using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers
{
    public class FPLController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
