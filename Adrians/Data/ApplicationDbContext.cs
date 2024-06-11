using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Adrians.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        [PersonalData]
        public string? Brukarnamn { get; set; }

        [PersonalData]
        public int? Poengsum { get; set; }
    }
}