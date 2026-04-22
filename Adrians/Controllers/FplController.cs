using System.Globalization;
using System.Text.Json;
using Adrians.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Adrians.Controllers;

public class FplController(IHttpClientFactory httpClientFactory, IMemoryCache cache) : Controller
{
    private const string CacheKey = "fpl_deadlines";
    private const string ApiUrl = "https://fantasy.premierleague.com/api/bootstrap-static/";

    public async Task<IActionResult> Index()
    {
        if (!cache.TryGetValue(CacheKey, out List<FplMatchesModel.Event>? events))
        {
            events = await FetchDeadlines();
            cache.Set(CacheKey, events, TimeSpan.FromHours(1));
        }

        var future = events!.Where(e => e.DeadlineTime > DateTime.UtcNow).ToList();
        return View(future);
    }

    private async Task<List<FplMatchesModel.Event>> FetchDeadlines()
    {
        var client = httpClientFactory.CreateClient();
        var json = await client.GetStringAsync(ApiUrl);
        var root = JsonSerializer.Deserialize<JsonElement>(json);

        return root.GetProperty("events")
            .EnumerateArray()
            .Select(e => new FplMatchesModel.Event
            {
                Id = e.GetProperty("id").GetInt32(),
                Name = e.GetProperty("name").GetString() ?? "",
                DeadlineTime = DateTime.Parse(
                    e.GetProperty("deadline_time").GetString()!,
                    null,
                    DateTimeStyles.AdjustToUniversal)
            })
            .ToList();
    }
}