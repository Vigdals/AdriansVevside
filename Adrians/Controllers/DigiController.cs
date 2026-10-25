using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Adrians.Controllers;

// All news-specific code lives here. Other controllers and the existing RSS service are untouched.
public sealed class DigiController(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    ILogger<DigiController> logger) : Controller
{
    private const int MaxFeedBytes = 2_000_000;
    private static readonly TimeSpan FreshFor = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan KeepLastGoodFor = TimeSpan.FromHours(12);

    // Fixed allowlist: feed addresses are never supplied by a browser user.
    private static readonly DigiFeed[] Feeds =
    [
        new("nsm", "NSM", "Varsel", "https://nsm.no/rss/sarbarhetsvarsler-fra-nsm/"),
        new("cisa", "CISA", "Varsel", "https://www.cisa.gov/cybersecurity-advisories/alerts.xml"),
        new("bleeping", "BleepingComputer", "Sikkerheit", "https://www.bleepingcomputer.com/feed/"),
        new("msrc", "Microsoft MSRC", "Sikkerheit", "https://api.msrc.microsoft.com/update-guide/rss"),
        new("krebs", "Krebs on Security", "Sikkerheit", "https://krebsonsecurity.com/feed/"),
        new("github", "GitHub Security", "Utvikling", "https://github.blog/security/feed/"),
        new("dotnet", ".NET Blog", "Utvikling", "https://devblogs.microsoft.com/dotnet/feed/")
    ];

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // A failed feed must never take down the other feeds or the view.
        var results = await Task.WhenAll(
            Feeds.Select(feed => ReadFeedAsync(feed, cancellationToken)));

        var articles = results
            .SelectMany(result => result.Articles)
            .GroupBy(article => article.Url, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderByDescending(article => article.PublishedAt ?? DateTimeOffset.MinValue)
            .Take(180)
            .ToList();

        var sources = results.Select(result => new DigiSourceStatus(
            result.Feed.Key,
            result.Feed.Name,
            result.Feed.Category,
            result.Feed.Url,
            result.Articles.Count,
            result.Available,
            result.Stale)).ToList();

        return View(new DigiNewsViewModel(articles, sources, DateTimeOffset.UtcNow));
    }

    private async Task<FeedResult> ReadFeedAsync(DigiFeed feed, CancellationToken cancellationToken)
    {
        var freshKey = $"digi:news:{feed.Key}:fresh";
        var lastGoodKey = $"digi:news:{feed.Key}:last-good";

        if (cache.TryGetValue(freshKey, out List<DigiArticle>? cached) && cached is not null)
        {
            return new FeedResult(feed, cached, Available: true, Stale: false);
        }

        try
        {
            using var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(8);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AdriansVevside/1.0 (RSS reader)");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/atom+xml, application/xml, text/xml");

            using var response = await client.GetAsync(
                feed.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentLength is > MaxFeedBytes)
            {
                throw new InvalidDataException("RSS response exceeds size limit.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                Async = true,
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = MaxFeedBytes,
                IgnoreComments = true
            });

            var document = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
            var articles = ParseArticles(document, feed).Take(35).ToList();

            if (articles.Count == 0)
            {
                throw new InvalidDataException("RSS response has no usable articles.");
            }

            cache.Set(freshKey, articles, FreshFor);
            cache.Set(lastGoodKey, articles, KeepLastGoodFor);

            return new FeedResult(feed, articles, Available: true, Stale: false);
        }
        catch (Exception ex) when (
            !cancellationToken.IsCancellationRequested &&
            ex is HttpRequestException or TaskCanceledException or XmlException or InvalidDataException)
        {
            logger.LogWarning(ex, "Could not read Digi news feed {Feed}", feed.Name);

            if (cache.TryGetValue(lastGoodKey, out List<DigiArticle>? lastGood) && lastGood is not null)
            {
                return new FeedResult(feed, lastGood, Available: false, Stale: true);
            }

            return new FeedResult(feed, [], Available: false, Stale: false);
        }
    }

    private static IEnumerable<DigiArticle> ParseArticles(XDocument document, DigiFeed feed)
    {
        // Both RSS 2.0 (<item>) and Atom (<entry>) are supported.
        foreach (var entry in document.Descendants()
                     .Where(element => element.Name.LocalName is "item" or "entry"))
        {
            var title = PlainText(ElementText(entry, "title"), 220);
            var url = ArticleUrl(entry);
            if (string.IsNullOrWhiteSpace(title) || url is null)
            {
                continue;
            }

            var description = PlainText(
                ElementText(entry, "description")
                ?? ElementText(entry, "summary")
                ?? ElementText(entry, "content"), 260);

            var dateText = ElementText(entry, "pubDate")
                ?? ElementText(entry, "published")
                ?? ElementText(entry, "updated")
                ?? ElementText(entry, "date");

            DateTimeOffset? publishedAt = DateTimeOffset.TryParse(
                dateText, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                out var parsedDate) ? parsedDate.ToUniversalTime() : null;

            yield return new DigiArticle(
                title, description, url, feed.Key, feed.Name, feed.Category, publishedAt);
        }
    }

    private static string? ArticleUrl(XElement entry)
    {
        var links = entry.Elements().Where(element => element.Name.LocalName == "link");
        var atomLink = links.FirstOrDefault(element =>
            string.Equals((string?)element.Attribute("rel"), "alternate", StringComparison.OrdinalIgnoreCase))
            ?? links.FirstOrDefault(element => element.Attribute("href") is not null);

        var candidate = (string?)atomLink?.Attribute("href")
            ?? links.FirstOrDefault()?.Value.Trim();

        // Do not output javascript:, data:, or other untrusted schemes into hrefs.
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
        {
            return null;
        }

        return uri.AbsoluteUri;
    }

    private static string? ElementText(XElement entry, string name) =>
        entry.Elements().FirstOrDefault(element => element.Name.LocalName == name)?.Value;

    private static string PlainText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var text = WebUtility.HtmlDecode(value);
        text = Regex.Replace(text, "<[^>]*>", " ", RegexOptions.Singleline);
        text = Regex.Replace(text, @"\s+", " ").Trim();

        return text.Length <= maxLength ? text : text[..maxLength].TrimEnd() + "…";
    }

    private sealed record DigiFeed(string Key, string Name, string Category, string Url);
    private sealed record FeedResult(
        DigiFeed Feed, List<DigiArticle> Articles, bool Available, bool Stale);
}

public sealed record DigiArticle(
    string Title,
    string Description,
    string Url,
    string SourceKey,
    string SourceName,
    string Category,
    DateTimeOffset? PublishedAt);

public sealed record DigiSourceStatus(
    string Key,
    string Name,
    string Category,
    string FeedUrl,
    int ArticleCount,
    bool Available,
    bool Stale);

public sealed record DigiNewsViewModel(
    IReadOnlyList<DigiArticle> Articles,
    IReadOnlyList<DigiSourceStatus> Sources,
    DateTimeOffset LoadedAt);
