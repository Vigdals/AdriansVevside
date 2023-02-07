using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers
{
    public class ResultatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}