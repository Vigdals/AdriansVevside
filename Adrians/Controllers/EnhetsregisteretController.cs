using Adrians.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Adrians.Controllers
{
    public class EnhetsregisteretController : Controller
    {
        public IActionResult Index(string orgNummer)
        {
            if (string.IsNullOrWhiteSpace(orgNummer))
            {
                return View();
            }
            else
            {
                var Organisasjon = GetApiInfo("https://data.brreg.no/enhetsregisteret/api/enheter/" + orgNummer);

                return View();
            }
        }

        public async Task<List<OrgModel>> GetApiInfo(string apiEndpoint)
        {
            var OrgModelList = new List<OrgModel>();
            var jsonResult = await ApiCall.DoApiCallAsync(apiEndpoint);

            JsonElement json = JsonSerializer.Deserialize<JsonElement>(jsonResult);


            return OrgModelList;
        }
    }
}
