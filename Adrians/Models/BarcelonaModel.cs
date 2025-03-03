using System;
using System.Collections.Generic;

namespace Adrians.Models
{
    public class Match
    {
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string HomeTeamLogo { get; set; }  // Logo URL for the home team
        public string AwayTeamLogo { get; set; }  // Logo URL for the away team
    }

}