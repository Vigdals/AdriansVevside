using Microsoft.EntityFrameworkCore;
using Adrians.Models.RPGModels;

namespace Adrians.Data
{
    public class GameContext : DbContext
    {
        public GameContext(DbContextOptions<GameContext> options)
            : base(options)
        {
        }

        public DbSet<Character> Characters { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<Location> Locations { get; set; }
    }
}