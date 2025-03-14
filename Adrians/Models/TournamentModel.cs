namespace Adrians.Models;

public class TournamentModel
{
    public class Championship
    {
        public string name { get; set; }
        public string fullName { get; set; }
        public int yearStart { get; set; }
        public int yearEnd { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public int id { get; set; }
        public object externalIds { get; set; }
        public int sportId { get; set; }
    }

    public class Data
    {
        public bool ratings { get; set; }
        public bool assists { get; set; }
        public bool attendances { get; set; }
        public bool corners { get; set; }
        public bool goalscorers { get; set; }
        public bool halfTimeScore { get; set; }
        public bool minutesPlayed { get; set; }
        public bool penalties { get; set; }
        public bool redCards { get; set; }
        public bool referees { get; set; }
        public bool shots { get; set; }
        public bool yellowCards { get; set; }
        public bool indirectAssists { get; set; }
        public bool transfers { get; set; }
        public bool robot { get; set; }
        public bool headCoaches { get; set; }
    }

    public class Root
    {
        public string name { get; set; }
        public string fullName { get; set; }
        public string groupName { get; set; }
        public int yearStart { get; set; }
        public int yearEnd { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }
        public Tournament tournament { get; set; }
        public int stageTypeId { get; set; }
        public int visibilityId { get; set; }
        public List<Championship> championships { get; set; }
        public Data data { get; set; }
        public string comment { get; set; }
        public object names { get; set; }
        public int numberOfRounds { get; set; }
        public int numberOfMeetingsBetweenTeams { get; set; }
        public object maxRoundNumber { get; set; }
        public bool active { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public int id { get; set; }
        public int sportId { get; set; }
    }

    public class Tournament
    {
        public string name { get; set; }
        public object level { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public int id { get; set; }
        public int sportId { get; set; }
    }
}