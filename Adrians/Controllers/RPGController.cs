using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers
{
    public class RPGController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
