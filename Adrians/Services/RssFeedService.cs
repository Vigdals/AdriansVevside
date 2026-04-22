// Services/RssFeedService.cs
using Adrians.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Xml.Linq;

namespace Adrians.Services;

public class RssFeedService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
{
    public async Task<List<RssItem>> GetItemsAsync(string feedUrl, TimeSpan? cacheDuration = null)
    {
        var cacheKey = $"rss_{feedUrl}";

        if (cache.TryGetValue(cacheKey, out List<RssItem>? cached))
            return cached!;

        var client = httpClientFactory.CreateClient();
        var xml = await client.GetStringAsync(feedUrl);

        var doc = XDocument.Parse(xml);

        var items = doc.Descendants("item").Select(i => new RssItem
        {
            Title = i.Element("title")?.Value ?? "",
            Link = i.Element("link")?.Value ?? "",
            Description = i.Element("description")?.Value ?? "",
            PubDate = DateTime.TryParse(i.Element("pubDate")?.Value, out var d) ? d : DateTime.MinValue,
            IsPremium = i.Element("isPremium")?.Value == "true",
            ImageUrl = i.Element("enclosure")?.Attribute("url")?.Value
        }).ToList();

        cache.Set(cacheKey, items, cacheDuration ?? TimeSpan.FromMinutes(30));
        return items;
    }
}