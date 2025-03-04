using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adrians.Models
{
    public class BarcelonaModel
    {
        public List<MatchDetail> Matches { get; set; }
    }

    public class MatchDetail
    {
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public string utcDate { get; set; }
        public string Status { get; set; }
    }

    public class Team
    {
        public string Name { get; set; }
        [JsonProperty("crestUrl")]
        public string CrestUrl { get; set; } // Property for the team logo URL
    }

    public class Match
    {
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string HomeTeamLogo { get; set; }  // Logo URL for the home team
        public string AwayTeamLogo { get; set; }  // Logo URL for the away team
    }

    public class Player
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }
    }
}