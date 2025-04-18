﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers;

public class AdminController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [Authorize(Roles = "Administrator")]
    public IActionResult Index()
    {
        var roles = _roleManager.Roles.ToList();

        return View(roles);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var role = new IdentityRole();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(RoleModel role)
    {
        if (ModelState.IsValid)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(role.Name));
            if (result.Succeeded) return RedirectToAction("Index");

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        }

        return View(role);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();
        return View(new RoleModel { Id = role.Id, Name = role.Name });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(RoleModel role)
    {
        if (ModelState.IsValid)
        {
            var result = await _roleManager.UpdateAsync(new IdentityRole(role.Name) { Id = role.Id });
            if (result.Succeeded) return RedirectToAction("Index");

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        }

        return View(role);
    }
}