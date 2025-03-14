using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Adrians.Models.RPGModels
{
    public class Character
    {
        [Key]
        public int CharacterId { get; set; }
        public string Name { get; set; }
        public int Health { get; set; }

        // Link to the Identity user (string is typical for ASP.NET Core Identity)
        public string ApplicationUserId { get; set; }

        // Navigation properties (optional)
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<Quest> Quests { get; set; }

        public Character()
        {
            Items = new List<Item>();
            Quests = new List<Quest>();
        }
    }
}