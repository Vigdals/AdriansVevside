using Markdig;

namespace Adrians.ViewModels
{
    public class BarcaViewModel
    {
        public List<Match> Matches { get; set; }
        public string GptSummary { get; set; }

        public string GptSummaryHtml =>
            string.IsNullOrWhiteSpace(GptSummary)
                ? string.Empty
                : Markdown.ToHtml(GptSummary);
    }
}
