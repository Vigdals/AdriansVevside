using Adrians.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace Adrians.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [AutoValidateAntiforgeryToken]
        public IActionResult Index()
        {
            Debug.WriteLine("HEYA");
            return View();
            //return RedirectToAction("Index", "EM24");
        }

        public IActionResult LogHubSite()
        {
            LogHub();
            return View("Index");
        }
        static async Task LogHub()
        {
            var conn = new HubConnectionBuilder()
                .WithUrl("https://loghub.statsforvalteren.no/messageHub")
                .Build();

            await conn.StartAsync();

            await conn.InvokeAsync("SendMessage", "Vevside", "Bacon ipsum dolor amet filet mignon ribeye bresaola landjaeger fatback short loin, picanha short ribs pig meatball pork loin kielbasa. Pork chop shoulder pork loin, salami fatback short ribs ground round pork belly boudin. Ground round meatball t-bone pork leberkas burgdoggen hamburger spare ribs turducken shankle strip steak pork chop tail ham hock jowl. Tail jerky jowl pancetta chuck flank. Pastrami short ribs beef ribs swine, tongue ham frankfurter.", "warning");

            await conn.DisposeAsync();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}