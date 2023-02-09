using Microsoft.AspNetCore.Mvc;
using Adrians.Models;
using Adrians.Resources;
using System.Diagnostics;
using System.Text.Json;
using Adrians.ViewModels;

namespace Adrians.Controllers
{
    public class CL : Controller
    {
        public IActionResult Index()
        {
            var clMatchModels = GetChampionsLeagueInfo("https://api.nifs.no/stages/690256/matches/");
            var MatchViewModelList = new List<NifsKampViewModel>();

            foreach (var match in clMatchModels)
            {
                MatchViewModelList.Add(new NifsKampViewModel(match));
            }
            return View(MatchViewModelList);
        }
        public static List<NifsKampModel> GetChampionsLeagueInfo(string apiEndpoint)
        {
            // string apiUrl = "https://api.nifs.no/stages/690256/matches/";
            var jsonResult = ApiCall.DoApiCall(apiEndpoint);

            //The most sexy oneliner in the world!
            //
            var clMatchModel2 = JsonSerializer.Deserialize<List<NifsKampModel>>(jsonResult);
            
            return clMatchModel2;
        }
    }
}
