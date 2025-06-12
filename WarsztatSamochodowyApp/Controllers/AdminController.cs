using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarsztatSamochodowyApp.Data;
using WarsztatSamochodowyApp.Models;

namespace WarsztatSamochodowyApp.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
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
        {
            _logger.LogWarning("Nie znaleziono użytkownika o id: {0} podczas próby usunięcia.", id);
            return NotFound();
        }

        try
        {
            await _userManager.DeleteAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania użytkownika o id: {0}", id);
            ModelState.AddModelError("", "Wystąpił błąd podczas usuwania użytkownika.");
            return View("Users", _userManager.Users);
        }

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
        {
            _logger.LogWarning("Nie znaleziono użytkownika o id: {0} podczas edycji.", model.Id);
            return NotFound();
        }

        user.Email = model.Email;
        user.UserName = model.Email; // Albo inny logiczny update username

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Błąd aktualizacji użytkownika o id: {0}: {1}", user.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            ModelState.AddModelError("", "Błąd aktualizacji użytkownika");
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = selectedRoles.Except(userRoles);
        var rolesToRemove = userRoles.Except(selectedRoles);

        try
        {
            await _userManager.AddToRolesAsync(user, rolesToAdd);
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas aktualizacji ról użytkownika o id: {0}", user.Id);
            ModelState.AddModelError("", "Błąd podczas aktualizacji ról użytkownika.");
            return View(model);
        }

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
        {
            _logger.LogWarning("Próba utworzenia roli bez nazwy.");
            ModelState.AddModelError("", "Nazwa roli jest wymagana");
        }

        if (!ModelState.IsValid)
            return View();

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                _logger.LogError("Błąd podczas tworzenia roli: {0}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                ModelState.AddModelError("", "Błąd podczas tworzenia roli.");
                return View();
            }
        }

        return RedirectToAction(nameof(Roles));
    }

    // Usuń rolę
    [HttpPost]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            _logger.LogWarning("Nie znaleziono roli o id: {0} podczas próby usunięcia.", id);
            return NotFound();
        }

        try
        {
            await _roleManager.DeleteAsync(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania roli o id: {0}", id);
            ModelState.AddModelError("", "Błąd podczas usuwania roli.");
            return View("Roles", _roleManager.Roles);
        }

        return RedirectToAction(nameof(Roles));
    }
}