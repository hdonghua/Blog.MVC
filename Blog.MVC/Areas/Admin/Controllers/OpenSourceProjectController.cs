using Blog.MVC.IServices.OpenSource;
using Blog.MVC.IServices.OpenSource.Dtos;
using Blog.MVC.Models.Common;
using Blog.MVC.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class OpenSourceProjectController : Controller
{
    private readonly IOpenSourceProjectAppService _openSourceProjectAppService;

    public OpenSourceProjectController(IOpenSourceProjectAppService openSourceProjectAppService)
    {
        _openSourceProjectAppService = openSourceProjectAppService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationHelper.DefaultPageSize)
    {
        var result = await _openSourceProjectAppService.GetPagedListAsync(page, pageSize);
        ViewBag.Pagination = PaginationViewModel.From(result, "OpenSourceProject");
        return View(result);
    }

    [HttpGet]
    public IActionResult Create() => View(new OpenSourceProjectFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OpenSourceProjectFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _openSourceProjectAppService.CreateAsync(MapToCreateDto(model));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["Success"] = "开源项目创建成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var project = await _openSourceProjectAppService.GetAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        return View(MapToFormViewModel(project));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, OpenSourceProjectFormViewModel model)
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
            await _openSourceProjectAppService.UpdateAsync(MapToUpdateDto(model));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["Success"] = "开源项目更新成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(long id)
    {
        var project = await _openSourceProjectAppService.GetAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _openSourceProjectAppService.DeleteAsync(id);
        TempData["Success"] = "开源项目已删除";
        return RedirectToAction(nameof(Index));
    }

    private static OpenSourceProjectFormViewModel MapToFormViewModel(OpenSourceProjectDto project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        RepositoryUrl = project.RepositoryUrl,
        DemoUrl = project.DemoUrl,
        Language = project.Language,
        SortOrder = project.SortOrder,
        IsPublished = project.IsPublished
    };

    private static CreateOpenSourceProjectDto MapToCreateDto(OpenSourceProjectFormViewModel model) => new()
    {
        Name = model.Name,
        Description = model.Description,
        RepositoryUrl = model.RepositoryUrl,
        DemoUrl = model.DemoUrl,
        Language = model.Language,
        SortOrder = model.SortOrder,
        IsPublished = model.IsPublished
    };

    private static UpdateOpenSourceProjectDto MapToUpdateDto(OpenSourceProjectFormViewModel model) => new()
    {
        Id = model.Id!.Value,
        Name = model.Name,
        Description = model.Description,
        RepositoryUrl = model.RepositoryUrl,
        DemoUrl = model.DemoUrl,
        Language = model.Language,
        SortOrder = model.SortOrder,
        IsPublished = model.IsPublished
    };
}
