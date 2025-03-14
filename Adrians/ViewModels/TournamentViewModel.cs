namespace Adrians.ViewModels;

public class TournamentViewModel
{
    public TournamentViewModel(TournamentModel.Root Grupper)
    {
        gruppenamn = Grupper.groupName;
        id = Grupper.id;
        yearStart = Grupper.yearStart;
    }

    public string gruppenamn { get; set; }
    public int id { get; set; }
    public int yearStart { get; set; }
}