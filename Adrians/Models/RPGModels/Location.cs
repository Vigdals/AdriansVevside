using System.ComponentModel.DataAnnotations;

namespace Adrians.Models.RPGModels
{
    public class Location
    {
        [Key]
        public int LocationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}