using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Adrians.Areas.Identity.Data;

// Add profile data for application users by adding properties to the AdriansUser class
public class AdriansUser : IdentityUser
{
    [PersonalData]
    public string? Brukarnamn { get; set; }

    [PersonalData]
    public int? Poengsum{ get; set;}
}