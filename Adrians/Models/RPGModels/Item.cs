using System.ComponentModel.DataAnnotations;

namespace Adrians.Models.RPGModels
{
    public class Item
    {
        [Key]
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Optionally, link to a character if needed
        public int CharacterId { get; set; }
        public virtual Character Owner { get; set; }
    }
}