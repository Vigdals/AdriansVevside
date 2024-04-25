namespace Adrians.ViewModels
{
    public class TournamentViewModel
    {
        public string gruppenamn { get; set; }
        public int id { get; set; }
        public int yearStart { get; set; }

        public TournamentViewModel(TournamentModel.Root Grupper)
        {
            this.gruppenamn = Grupper.groupName;
            this.id = Grupper.id;
            this.yearStart = Grupper.yearStart;
        }
    }
}
