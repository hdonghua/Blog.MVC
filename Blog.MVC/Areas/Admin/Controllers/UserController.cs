using System.Security.Claims;
using Blog.MVC.IServices.Users;
using Blog.MVC.IServices.Users.Dtos;
using Blog.MVC.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class UserController : Controller
{
    private readonly IUserAppService _userAppService;

    public UserController(IUserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    public IActionResult Index()
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return RedirectToAction(nameof(Edit), new { id = userId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var currentUserId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (id != currentUserId)
        {
            return Forbid();
        }

        var user = await _userAppService.GetAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(new UserFormViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            DisplayName = user.DisplayName
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, UserFormViewModel model)
    {
        var currentUserId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (id != model.Id || id != currentUserId)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _userAppService.UpdateAsync(new UpdateUserDto
            {
                Id = id,
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                DisplayName = model.DisplayName
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["Success"] = "账号信息已更新";
        return RedirectToAction(nameof(Edit), new { id });
    }
}
