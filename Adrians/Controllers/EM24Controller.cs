using Adrians.Models;
using Adrians.Resources;
using Adrians.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using static Adrians.Models.TournamentModel;

namespace Adrians.Controllers
{
    public class EM24Controller : Controller
    {
        [Authorize]
        public IActionResult Index(string tournamentID)
        {
            tournamentID = "59";
            //Henter ut info om heile turneringa. Kvar gruppe har ein ID som treng eit api kall for å henta alle kampar
            var TournamentViewModelList = GetTournamentInfo(tournamentID);
            //Oppretta tom liste av kampar
            var matchViewModelList = new List<NifsKampViewModel>();
            //Går igjennom kvar gruppe og hentar ut alle kampar
            foreach (var tournamentViewModel in TournamentViewModelList)
            {
                var matchModels = GetKampInfo("https://api.nifs.no/stages/" + tournamentViewModel.id + "/matches/");

                //adding info just to display to the view
                foreach (var match in matchModels.Result)
                {
                    matchViewModelList.Add(new NifsKampViewModel(match));
                    
                    Debug.WriteLine(tournamentViewModel.gruppenamn + match.homeTeam.name + " + " + match.awayTeam.name);
                }
                //return View(matchViewModelList);
            }
            return View(matchViewModelList);
        }

        public List<TournamentViewModel> GetTournamentInfo(string tournamentID)
        {
            var tournamentModels = GetGruppeInfo("https://api.nifs.no/tournaments/" + tournamentID + "/stages/");

            var TournamentViewModelList = new List<TournamentViewModel>();

            //adding info just to display to the view
            foreach (var gruppe in tournamentModels.Result)
            {
                if (gruppe.yearStart == 2024)
                {
                    TournamentViewModelList.Add(new TournamentViewModel(gruppe));
                }
            }
            return TournamentViewModelList;
        }

        public async Task<List<TournamentModel.Root>> GetGruppeInfo(string apiEndpoint)
        {
            var jsonResult = await ApiCall.DoApiCallAsync(apiEndpoint);

            //The most sexy oneliner in the world!
            //Takes the jsonResult, deserializes it and adds it to my model. Crazy easy
            var tournamentModels = JsonSerializer.Deserialize<List<TournamentModel.Root>>(jsonResult);

            return tournamentModels;
        }

        public async Task<List<NifsKampModel>> GetKampInfo(string apiEndpoint)
        {
            // string apiUrl = "https://api.nifs.no/stages/690256/matches/";
            var jsonResult = await ApiCall.DoApiCallAsync(apiEndpoint);

            //The most sexy oneliner in the world!
            //Takes the jsonResult, deserializes it and adds it to my model. Crazy easy
            var matchModel = JsonSerializer.Deserialize<List<NifsKampModel>>(jsonResult);

            return matchModel;
        }
    }
}
