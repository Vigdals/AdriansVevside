using static Adrians.Models.FplMatchesModel;

namespace Adrians.Models
{
    
    public class HackerNewsModel
    {
        public string? by { get; set; }
        public int? descendants { get; set; }
        public int? id { get; set; }
        public List<int>? kids { get; set; }
        public int? score { get; set; }
        public int? time { get; set; }
        public string? title { get; set; }
        public string? type { get; set; }
        public string? url { get; set; }


    }
    public class ApiResponse
    {
        public List<Event> Events { get; set; }
        // ... other properties if needed
    }
}
