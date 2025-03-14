﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Adrians.Data;
using Adrians.Models.RPGModels;
using Adrians.ViewModels;
using System;

namespace Adrians.Controllers
{
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
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the user already has a character
            if (_context.Characters.Any(c => c.ApplicationUserId == userId))
            {
                return RedirectToAction("Play");
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

            return RedirectToAction("Play");
        }

        // GET: Game/Play
        public IActionResult Play()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var character = _context.Characters.FirstOrDefault(c => c.ApplicationUserId == userId);

            if (character == null)
            {
                // If no character exists, redirect to create one.
                return RedirectToAction("CreateCharacter");
            }

            // Pass the character to the view so you can display its details
            return View("~/Views/RPG/Play.cshtml", character);
        }
        private static readonly Random _random = new Random();

        public IActionResult Adventure()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var character = _context.Characters.FirstOrDefault(c => c.ApplicationUserId == userId);

            if (character == null)
                return Content("No character found. Please create one first.", "text/plain");

            // Introduce the enemy
            string enemyName = "a feeble Orc";
            string enemyDescription = "The enemy is clearly weaker than you, with a dented helmet and a splintered shield.";
            string encounterMessage = $"You encounter {enemyName}. {enemyDescription}";

            // Simulate battle outcome: 75% chance to win.
            double playerChanceToWin = 0.75;
            double roll = _random.NextDouble();

            if (roll < playerChanceToWin)
            {
                int healthGain = 10;
                character.Health += healthGain;
                _context.SaveChanges();
                return Content($"{encounterMessage}\n\nYou engage in battle and defeat the enemy! You gain {healthGain} health. New health: {character.Health}.", "text/plain");
            }
            else
            {
                int healthLoss = 15;
                character.Health -= healthLoss;
                _context.SaveChanges();
                return Content($"{encounterMessage}\n\nYou fight valiantly but take a hit, losing {healthLoss} health. New health: {character.Health}.", "text/plain");
            }
        }
    }
}
