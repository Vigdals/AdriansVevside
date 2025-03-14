using System.ComponentModel.DataAnnotations;

namespace Adrians.Models.RPGModels
{
    public class Quest
    {
        [Key]
        public int QuestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }

        // Optionally link to a character
        public int CharacterId { get; set; }
        public virtual Character AssignedTo { get; set; }
    }
}