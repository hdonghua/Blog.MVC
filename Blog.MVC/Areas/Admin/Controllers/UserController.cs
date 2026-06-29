using Blog.MVC.IServices.Users;
using Blog.MVC.IServices.Users.Dtos;
using Blog.MVC.Models.Users;
using Blog.MVC.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class UserController : Controller
{
    private readonly IUserAppService _userAppService;

    public UserController(IUserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userAppService.GetListAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create() => View(new UserFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "创建用户时必须设置密码");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _userAppService.CreateAsync(new CreateUserDto
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password!,
                DisplayName = model.DisplayName,
                Role = model.Role,
                IsActive = model.IsActive
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["Success"] = "用户创建成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
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
            DisplayName = user.DisplayName,
            Role = user.Role,
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, UserFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
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
                DisplayName = model.DisplayName,
                Role = model.Role,
                IsActive = model.IsActive
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["Success"] = "用户更新成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await _userAppService.GetAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var currentUserId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        if (id == currentUserId)
        {
            TempData["Error"] = "不能删除当前登录用户";
            return RedirectToAction(nameof(Index));
        }

        await _userAppService.DeleteAsync(id);
        TempData["Success"] = "用户已删除";
        return RedirectToAction(nameof(Index));
    }
}
