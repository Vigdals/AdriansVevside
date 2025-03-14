using System.Diagnostics;
using System.Text.Json;
using Adrians.Resources;
using Adrians.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers;

public class KampInfoController : Controller
{
    public IActionResult Index(string stageId)
    {
        Debug.WriteLine(stageId);
        if (string.IsNullOrWhiteSpace(stageId)) return View();

        var matchModels = GetKampInfo("https://api.nifs.no/stages/" + stageId + "/matches/");
        //var matchModels = GetKampInfo("https://api.nifs.no/stages/690256/matches/");
        var matchViewModelList = new List<NifsKampViewModel>();

        //adding info just to display to the view
        foreach (var match in matchModels.Result) matchViewModelList.Add(new NifsKampViewModel(match));
        return View(matchViewModelList);
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