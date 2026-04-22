// Controllers/HackerNews.cs
using Adrians.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Adrians.Controllers;

public class HackerNews(IHttpClientFactory httpClientFactory, IMemoryCache cache) : Controller
{
    private const string CacheKey = "hackernews_top";

    public async Task<IActionResult> Index()
    {
        if (!cache.TryGetValue(CacheKey, out List<HackerNewsModel>? items))
        {
            items = await FetchTopStories();
            cache.Set(CacheKey, items, TimeSpan.FromMinutes(15));
        }

        return View(items);
    }

    private async Task<List<HackerNewsModel>> FetchTopStories()
    {
        var client = httpClientFactory.CreateClient("hackernews");

        var idsJson = await client.GetStringAsync(
            "https://hacker-news.firebaseio.com/v0/topstories.json");

        var ids = JsonSerializer.Deserialize<int[]>(idsJson)!
            .Take(10);

        // Alle 10 kall parallelt
        var tasks = ids.Select(id =>
            client.GetStringAsync($"https://hacker-news.firebaseio.com/v0/item/{id}.json"));

        var results = await Task.WhenAll(tasks);

        return results
            .Select(json => ParseStory(json))
            .Where(m => m is not null)
            .OrderByDescending(m => m!.score)
            .ToList()!;
    }

    private static HackerNewsModel? ParseStory(string json)
    {
        try
        {
            var el = JsonSerializer.Deserialize<JsonElement>(json);
            return new HackerNewsModel
            {
                id = el.GetProperty("id").GetInt32(),
                title = el.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                url = el.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "",
                by = el.TryGetProperty("by", out var b) ? b.GetString() ?? "" : "",
                score = el.TryGetProperty("score", out var s) ? s.GetInt32() : 0,
                descendants = el.TryGetProperty("descendants", out var d) ? d.GetInt32() : 0,
            };
        }
        catch
        {
            return null;
        }
    }
}