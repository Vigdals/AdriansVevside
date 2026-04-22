namespace Adrians.Models;

public class RssItem
{
    public string Title { get; set; } = "";
    public string Link { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime PubDate { get; set; }
    public bool IsPremium { get; set; }
    public string? ImageUrl { get; set; }
}