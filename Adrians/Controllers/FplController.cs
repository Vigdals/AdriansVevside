using Adrians.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Adrians.Controllers
{
    public class FplController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            var hackerNewsModelList = await GetDeadlines();


            //Just getting future deadlines
            DateTime now = DateTime.UtcNow;
            List<FplMatchesModel.Event> futureDeadlines = new List<FplMatchesModel.Event>();
            foreach (var eventItem in hackerNewsModelList)
            {
                if (eventItem.DeadlineTime > now)
                {
                    futureDeadlines.Add(eventItem);
                }
            }

            return View(futureDeadlines);
        }

        public async Task<List<FplMatchesModel.Event>> GetDeadlines()
        {
            var fplMatchesModelList = new List<FplMatchesModel.Event>();

            string apiUrL = "https://fantasy.premierleague.com/api/bootstrap-static/";
            var jsonResult = await ApiCall.DoApiCallAsync(apiUrL);
            JsonElement bigFplJson = JsonSerializer.Deserialize<JsonElement>(jsonResult);
            JsonElement events = bigFplJson.GetProperty("events");

            foreach (JsonElement eventElement in events.EnumerateArray())
            {
                var eventId = eventElement.GetProperty("id").GetInt32();
                var eventName = eventElement.GetProperty("name").GetString();
                var deadlineTime = DateTime.Parse(eventElement.GetProperty("deadline_time").GetString(), null, System.Globalization.DateTimeStyles.AdjustToUniversal);

                var fplEvent = new FplMatchesModel.Event
                {
                    Id = eventId,
                    Name = eventName,
                    DeadlineTime = deadlineTime
                };
                fplMatchesModelList.Add(fplEvent);
            }
            return fplMatchesModelList;
        }
    }
}
