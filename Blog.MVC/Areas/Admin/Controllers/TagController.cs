using Blog.MVC.IServices.Blog;
using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Models.Common;
using Blog.MVC.Models.Users;
using Blog.MVC.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Author)}")]
public class TagController : Controller
{
    private readonly ITagAppService _tagAppService;

    public TagController(ITagAppService tagAppService)
    {
        _tagAppService = tagAppService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationHelper.DefaultPageSize)
    {
        var result = await _tagAppService.GetPagedListAsync(page, pageSize);
        ViewBag.Pagination = PaginationViewModel.From(result, "Tag");
        return View(result);
    }

    [HttpGet]
    public IActionResult Create() => View(new TagFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TagFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _tagAppService.CreateAsync(new CreateTagDto
            {
                Name = model.Name,
                Slug = model.Slug
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["Success"] = "标签创建成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var tag = await _tagAppService.GetAsync(id);
        if (tag == null)
        {
            return NotFound();
        }

        return View(new TagFormViewModel
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, TagFormViewModel model)
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
            await _tagAppService.UpdateAsync(new UpdateTagDto
            {
                Id = id,
                Name = model.Name,
                Slug = model.Slug
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["Success"] = "标签更新成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(long id)
    {
        var tag = await _tagAppService.GetAsync(id);
        if (tag == null)
        {
            return NotFound();
        }

        return View(tag);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            await _tagAppService.DeleteAsync(id);
            TempData["Success"] = "标签已删除";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
