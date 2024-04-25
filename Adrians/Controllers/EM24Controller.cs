using Adrians.Models;
using Adrians.Resources;
using Adrians.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Adrians.Controllers
{
    public class EM24Controller : Controller
    {
        //[Authorize]
        public IActionResult Index(string tournamentID)
        {
            tournamentID = "59";

            Debug.WriteLine(tournamentID);
            if (string.IsNullOrWhiteSpace(tournamentID))
            {
                return View();
            }
            else
            {
                var tournamentModels = GetGruppeInfo("https://api.nifs.no/tournaments/" + tournamentID + "/stages/");
                //var matchModels = GetKampInfo("https://api.nifs.no/stages/690256/matches/");


                var TournamentViewModelList = new List<TournamentViewModel>();

                //adding info just to display to the view
                foreach (var gruppe in tournamentModels.Result)
                {
                    if (gruppe.yearStart == 2024)
                    {
                        TournamentViewModelList.Add(new TournamentViewModel(gruppe));
                    }
                }
                return View(TournamentViewModelList);
            }

        }

        public async Task<List<TournamentModel.Root>> GetGruppeInfo(string apiEndpoint)
        {
            var jsonResult = await ApiCall.DoApiCallAsync(apiEndpoint);

            //The most sexy oneliner in the world!
            //Takes the jsonResult, deserializes it and adds it to my model. Crazy easy
            var tournamentModels = JsonSerializer.Deserialize<List<TournamentModel.Root>>(jsonResult);

            return tournamentModels;
        }
    }
}
