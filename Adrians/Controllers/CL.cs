using Microsoft.AspNetCore.Mvc;
using Adrians.Models;
using Adrians.Resources;
using System.Diagnostics;
using System.Text.Json;

namespace Adrians.Controllers
{
    public class CL : Controller
    {
        public IActionResult Index()
        {
            GetChampionsLeagueInfo("https://api.nifs.no/stages/690256/matches/");
            return View();
        }
        public static List<CLMatchModel> GetChampionsLeagueInfo(string apiEndpoint)
        {
            var ListOfCLMatchModels = new List<CLMatchModel>();

            // string apiUrl = "https://api.nifs.no/stages/690256/matches/";
            var jsonResult = ApiCall.DoApiCall(apiEndpoint);

            JsonElement jsonElementMatches = JsonSerializer.Deserialize<JsonElement>(jsonResult);

            foreach (JsonElement match in jsonElementMatches.EnumerateArray())
            {
                Console.WriteLine("-------------");
                Console.WriteLine($"Match ID: {match.GetProperty("id").GetInt32()}");
                JsonElement homeTeam = match.GetProperty("homeTeam");
                Console.WriteLine($"Home Team: {homeTeam.GetProperty("name").GetString()}");
                JsonElement awayTeam = match.GetProperty("awayTeam");
                Console.WriteLine($"Away Team: {awayTeam.GetProperty("name").GetString()}");
                Console.WriteLine($"Match date: {match.GetProperty("timestamp").GetString()}");
            }

            return ListOfCLMatchModels;
        }
    }
}
