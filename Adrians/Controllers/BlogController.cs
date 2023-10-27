using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
