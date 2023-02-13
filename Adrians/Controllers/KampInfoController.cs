using Microsoft.AspNetCore.Mvc;
using Adrians.Models;
using Adrians.Resources;
using System.Diagnostics;
using System.Text.Json;
using Adrians.ViewModels;

namespace Adrians.Controllers
{
    public class KampInfoController : Controller
    {
        public IActionResult Index(string stageId)
        {
            Debug.WriteLine(stageId);
            if (string.IsNullOrWhiteSpace(stageId))
            {
                return View();
            }
            else
            {
                var matchModels = GetKampInfo("https://api.nifs.no/stages/" + stageId + "/matches/");
                //var matchModels = GetKampInfo("https://api.nifs.no/stages/690256/matches/");
                var matchViewModelList = new List<NifsKampViewModel>();


                //adding info just to display to the view
                foreach (var match in matchModels)
                {
                    matchViewModelList.Add(new NifsKampViewModel(match));
                }
                return View(matchViewModelList);
            }

        }
        public static List<NifsKampModel> GetKampInfo(string apiEndpoint)
        {
            // string apiUrl = "https://api.nifs.no/stages/690256/matches/";
            var jsonResult = ApiCall.DoApiCall(apiEndpoint);

            //The most sexy oneliner in the world!
            //Takes the jsonResult, deserializes it and adds it to my model. Crazy easy
            var matchModel = JsonSerializer.Deserialize<List<NifsKampModel>>(jsonResult);
            
            return matchModel;
        }
    }
}
