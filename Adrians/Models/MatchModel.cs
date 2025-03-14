namespace Adrians.Models;

public class MatchModel
{
    public int Id { get; set; }

    public class AwayTeam
    {
        public Logo logo { get; set; }
        public object matchStatistics { get; set; }
        public string name { get; set; }
        public object teamPhoto { get; set; }
        public object names { get; set; }
        public List<Club> clubs { get; set; }
        public string shortName { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public int id { get; set; }
        public ExternalIds externalIds { get; set; }
    }

    public class Club
    {
        public string name { get; set; }
        public string primaryColor { get; set; }
        public string secondaryColor { get; set; }
        public string tertiaryColor { get; set; }
        public string textColor { get; set; }
        public string address { get; set; }
        public string homePage { get; set; }
        public string dateFounded { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public int id { get; set; }
        public int sportId { get; set; }
    }

    public class ExternalIds
    {
        public List<int> tv2 { get; set; }
    }

    public class HomeTeam
    {
        public Logo logo { get; set; }
        public object matchStatistics { get; set; }
        public string name { get; set; }
        public object teamPhoto { get; set; }
        public object names { get; set; }
        public List<Club> clubs { get; set; }
        public string shortName { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public int id { get; set; }
        public ExternalIds externalIds { get; set; }
    }

    public class LiveFeed
    {
        public object customerId { get; set; }
        public int matchId { get; set; }
        public bool prioritized { get; set; }
        public bool ended { get; set; }
        public string user { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public int id { get; set; }
        public object externalIds { get; set; }
        public int sportId { get; set; }
    }

    public class Logo
    {
        public string url { get; set; }
        public int imageTypeId { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public int id { get; set; }
    }

    public class Result
    {
        public int homeScore45 { get; set; }
        public int? homeScore90 { get; set; }
        public int awayScore45 { get; set; }
        public int? awayScore90 { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
    }

    public class Root
    {
        public DateTime timestamp { get; set; }
        public string name { get; set; }
        public Result result { get; set; }
        public HomeTeam homeTeam { get; set; }
        public AwayTeam awayTeam { get; set; }
        public int matchStatusId { get; set; }
        public int matchTypeId { get; set; }
        public Stadium stadium { get; set; }
        public int? attendance { get; set; }
        public int round { get; set; }
        public string comment { get; set; }
        public bool coveredLive { get; set; }
        public int stageId { get; set; }
        public string lastUpdated { get; set; }
        public string matchLength { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public int id { get; set; }
        public object externalIds { get; set; }
        public int sportId { get; set; }
        public List<LiveFeed> liveFeeds { get; set; }
    }

    public class Stadium
    {
        public string name { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public int id { get; set; }
        public int sportId { get; set; }
    }
}