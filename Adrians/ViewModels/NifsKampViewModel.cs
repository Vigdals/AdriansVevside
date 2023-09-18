using Adrians.Models;

namespace Adrians.ViewModels
{
    public class NifsKampViewModel
    {
        public string homeTeam { get; set; }
        public string awayTeam { get; set; }
        public string result { get; set; }
        public string stadium { get; set; }
        public NifsKampViewModel(NifsKampModel match)
        {
            this.homeTeam = match.homeTeam.name;
            this.awayTeam = match.awayTeam.name;
            this.result = match.result.homeScore90 + " - " + match.result.awayScore90;
            this.stadium = match.stadium.name;
        }
    }
    
}