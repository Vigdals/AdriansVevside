namespace Adrians.ViewModels;

public class NifsKampViewModel
{
    //public string group { get; set; }
    public NifsKampViewModel(NifsKampModel match)
    {
        homeTeam = match.homeTeam.name;
        awayTeam = match.awayTeam.name;
        result = match.result.homeScore90 + " - " + match.result.awayScore90;
        stadium = match.stadium.name;
    }

    public string homeTeam { get; set; }
    public string awayTeam { get; set; }
    public string result { get; set; }
    public string stadium { get; set; }
}