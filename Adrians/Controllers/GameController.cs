using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Adrians.Data;
using Adrians.Models.RPGModels;
using Adrians.ViewModels;
using System;

namespace Adrians.Controllers
{
    //[Route("https://vg.no/test")]
    [Authorize]
    public class GameController : Controller
    {
        private readonly GameContext _context;

        public GameController(GameContext context)
        {
            _context = context;
        }

        // GET: Game/CreateCharacter
        public IActionResult CreateCharacter()
        {
            return View("~/Views/RPG/CreateCharacter.cshtml");
        }

        // POST: Game/CreateCharacter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCharacter(CreateCharacterViewModel model)
        {
            if (string.IsNullOrEmpty(model.CharacterName))
            {
                ModelState.AddModelError("CharacterName", "Character name is required.");
                return View("~/Views/RPG/CreateCharacter.cshtml", model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the user already has a character
            if (_context.Characters.Any(c => c.ApplicationUserId == userId))
            {
                return RedirectToAction("Index", "RPG");
            }

            // Create and save the new character
            var character = new Character
            {
                Name = model.CharacterName,
                Health = 100,
                ApplicationUserId = userId
            };

            _context.Characters.Add(character);
            _context.SaveChanges();

            return RedirectToAction("Index", "RPG");
        }

        // GET: Game/Play

        private static readonly Random _random = new Random();

        /// <summary>
        /// GET: Game/Adventure
        /// Simulates a battle with one of three enemy types:
        /// - A feeble Orc (common, ~89% chance) with a 75% chance to win,
        /// - A mighty Uruk-hai (rare, 10% chance) with a 40% chance to win,
        /// - The Balrog (extremely rare, 1% chance) which always defeats you,
        /// returning a JSON result with a battle message and updated health.
        /// </summary>
        public IActionResult Adventure()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var character = _context.Characters.FirstOrDefault(c => c.ApplicationUserId == userId);

            if (character == null)
            {
                return Json(new { error = "No character found. Please create one first." });
            }

            // Roll to determine which enemy to encounter.
            // _random.NextDouble() returns a value between 0.0 and 1.0.
            double roll = _random.NextDouble();

            string enemyName;
            string enemyDescription;
            string encounterMessage;
            string resultMessage;

            // 1% chance: Encounter the Balrog.
            if (roll < 0.01)
            {
                enemyName = "the Balrog";
                enemyDescription = "a terrifying, ancient creature of fire and shadow.";
                encounterMessage = $"You encounter {enemyName}. {enemyDescription}";

                // The Balrog always wins: your health is set to 1.
                character.Health = 1;
                _context.SaveChanges();
                resultMessage = $"{encounterMessage}\n\nYou are overwhelmed by the Balrog and fall, telling the others to -- FLY YOU FOOLS --, leaving you with only 1 HP.";
            }
            // 10% chance: Encounter a mighty Uruk-hai.
            else if (roll < 0.11) // from 0.01 to 0.11 = 10%
            {
                enemyName = "a mighty Uruk-hai";
                enemyDescription = "a large and formidable warrior, much stronger than a feeble Orc.";
                encounterMessage = $"You encounter {enemyName}. {enemyDescription}";

                // For the mighty enemy, give you a lower chance to win (40% chance).
                double winChance = 0.40;
                double battleRoll = _random.NextDouble();
                if (battleRoll < winChance)
                {
                    int healthGain = 40;
                    character.Health = Math.Min(character.Health + healthGain, 100);
                    _context.SaveChanges();
                    resultMessage = $"{encounterMessage}\n\nAgainst all odds, you defeat the mighty enemy! You gain {healthGain} health. New health: {character.Health}.";
                }
                else
                {
                    int healthLoss = 30;
                    character.Health -= healthLoss;
                    _context.SaveChanges();
                    resultMessage = $"{encounterMessage}\n\nYou struggle fiercely, but the Uruk-hai overpowers you, and you lose {healthLoss} health. New health: {character.Health}.";
                }
            }
            // Otherwise (~89% chance): Encounter a feeble Orc.
            else
            {
                enemyName = "a feeble Orc";
                enemyDescription = "the enemy is clearly weaker than you, with a dented helmet and a splintered shield.";
                encounterMessage = $"You encounter {enemyName}. {enemyDescription}";

                // For the feeble enemy, you have a higher chance to win (75% chance).
                double winChance = 0.75;
                double battleRoll = _random.NextDouble();
                if (battleRoll < winChance)
                {
                    int healthGain = 10;
                    character.Health = Math.Min(character.Health + healthGain, 100);
                    _context.SaveChanges();
                    resultMessage = $"{encounterMessage}\n\nYou engage in battle and defeat the enemy! You gain {healthGain} health. New health: {character.Health}.";
                }
                else
                {
                    int healthLoss = 15;
                    character.Health -= healthLoss;
                    _context.SaveChanges();
                    resultMessage = $"{encounterMessage}\n\nYou fight valiantly but take a hit, losing {healthLoss} health. New health: {character.Health}.";
                }
            }

            return Json(new { message = resultMessage, health = character.Health });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetGame()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var character = _context.Characters.FirstOrDefault(c => c.ApplicationUserId == userId);

            int fullHealth = 100;
            character.Health = fullHealth;
            _context.SaveChanges();

            return Json(new {health = character.Health});
        }
    }
}
