using System.Text.Json;
using Adrians.Resources;
using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers;

public class EnhetsregisteretController : Controller
{
    public IActionResult Index(string orgNummer)
    {
        if (string.IsNullOrWhiteSpace(orgNummer)) return View();

        var Organisasjon = GetApiInfo("https://data.brreg.no/enhetsregisteret/api/enheter/" + orgNummer);

        return View();
    }

    public async Task<List<OrgModel>> GetApiInfo(string apiEndpoint)
    {
        var OrgModelList = new List<OrgModel>();
        var jsonResult = await ApiCall.DoApiCallAsync(apiEndpoint);

        var json = JsonSerializer.Deserialize<JsonElement>(jsonResult);


        return OrgModelList;
    }
}