using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Adrians.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        //[PersonalData]
        //public string? Brukarnamn { get; set; }

        //[PersonalData]
        //public int? Poengsum { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
        {
        }
    }
}