using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniErp.Models;
using MiniErp.Security;
using MiniErp.ViewModels;

namespace MiniErp.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class UsersController(UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new List<UserListItemViewModel>();
        foreach (var user in userManager.Users.OrderBy(user => user.Email))
        {
            model.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = string.Join(", ", await userManager.GetRolesAsync(user)),
                IsLocked = user.LockoutEnd > DateTimeOffset.UtcNow
            });
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        LoadRoles();
        return View(new CreateUserViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!AppRoles.All.Contains(model.Role))
            ModelState.AddModelError(nameof(model.Role), "Rol no válido.");

        if (!ModelState.IsValid)
        {
            LoadRoles(model.Role);
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            LockoutEnabled = true
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            var roleResult = await userManager.AddToRoleAsync(user, model.Role);
            if (roleResult.Succeeded)
                return RedirectToAction(nameof(Index));

            await userManager.DeleteAsync(user);
            AddErrors(roleResult);
        }
        else
        {
            AddErrors(result);
        }

        LoadRoles(model.Role);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        var currentUser = await userManager.GetUserAsync(User);
        if (user is null)
            return NotFound();

        if (user.Id == currentUser?.Id)
        {
            TempData["Error"] = "No puedes bloquear tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        DateTimeOffset? lockoutEnd = user.LockoutEnd > DateTimeOffset.UtcNow ? null : DateTimeOffset.MaxValue;
        var result = await userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        if (!result.Succeeded)
            TempData["Error"] = string.Join("; ", result.Errors.Select(error => error.Description));

        return RedirectToAction(nameof(Index));
    }

    private void LoadRoles(string? selected = null) =>
        ViewBag.Roles = new SelectList(AppRoles.All, selected);

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }
}
