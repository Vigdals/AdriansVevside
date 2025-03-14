namespace Adrians.Models;

public class FplMatchesModel
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DeadlineTime { get; set; }
    }
}