using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

[Authorize(Policy = "OnlyAdmins")]
public class AdminController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    // Lista użytkowników
    public IActionResult Users()
    {
        var users = _userManager.Users;
        return View(users);
    }

    // Usuń użytkownika
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        await _userManager.DeleteAsync(user);
        return RedirectToAction(nameof(Users));
    }

    // Edytuj użytkownika (formularz)
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            Roles = roles
        };

        ViewBag.AllRoles = await _roleManager.Roles.ToListAsync();
        return View(model);
    }

    // Edytuj użytkownika (POST)
    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserViewModel model, string[] selectedRoles)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
            return NotFound();

        user.Email = model.Email;
        user.UserName = model.Email; // Albo inny logiczny update username

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            ModelState.AddModelError("", "Błąd aktualizacji użytkownika");

        var userRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = selectedRoles.Except(userRoles);
        var rolesToRemove = userRoles.Except(selectedRoles);

        await _userManager.AddToRolesAsync(user, rolesToAdd);
        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

        return RedirectToAction(nameof(Users));
    }

    // Lista ról
    public IActionResult Roles()
    {
        var roles = _roleManager.Roles;
        return View(roles);
    }

    // Dodaj rolę (GET)
    public IActionResult CreateRole()
    {
        return View();
    }

    // Dodaj rolę (POST)
    [HttpPost]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            ModelState.AddModelError("", "Nazwa roli jest wymagana");

        if (!ModelState.IsValid)
            return View();

        if (!await _roleManager.RoleExistsAsync(roleName)) await _roleManager.CreateAsync(new IdentityRole(roleName));

        return RedirectToAction(nameof(Roles));
    }

    // Usuń rolę
    [HttpPost]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        await _roleManager.DeleteAsync(role);
        return RedirectToAction(nameof(Roles));
    }
}