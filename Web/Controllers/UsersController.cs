using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;

namespace Web.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _userService.GetAllAsync());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new UserCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Dto.Password))
        {
            ModelState.AddModelError("Dto.Password", "Password is required.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await _userService.CreateAsync(model.Dto, model.Dto.Password!);
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user is null ? NotFound() : View(new UserIndexViewModel { User = user });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(new UserEditViewModel
        {
            Id = user.Id,
            Dto = new UpdateUserDto(user.FullName, user.UserName, user.Email, null)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _userService.UpdateAsync(model.Id, model.Dto);
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user is null ? NotFound() : View(new UserIndexViewModel { User = user });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, bool confirmed)
    {
        if (!confirmed)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            await _userService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            TempData["Error"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
