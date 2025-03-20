using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Adrians.Data;
using Adrians.Models.RPGModels;

namespace Adrians.Controllers
{
    [Authorize]
    public class RPGController : Controller
    {
        private readonly GameContext _context;

        public RPGController(GameContext context)
        {
            _context = context;
        }

        // GET: /RPG
        public IActionResult Index()
        {
            // Get the current user's id
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Query for the character that belongs to the user
            var character = _context.Characters.FirstOrDefault(c => c.ApplicationUserId == userId);
            if (character == null)
            {
                // If no character exists, redirect to Game/CreateCharacter
                return RedirectToAction("CreateCharacter", "Game");
            }
            // Pass the character to the view
            return View(character);
        }
    }
}