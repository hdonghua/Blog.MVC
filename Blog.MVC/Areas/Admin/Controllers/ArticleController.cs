using Blog.MVC.IServices.Blog;
using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Models.Blog;
using Blog.MVC.Models.Common;
using Blog.MVC.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Blog.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ArticleController : Controller
{
    private readonly IArticleAppService _articleAppService;
    private readonly ICategoryAppService _categoryAppService;
    private readonly ITagAppService _tagAppService;

    public ArticleController(
        IArticleAppService articleAppService,
        ICategoryAppService categoryAppService,
        ITagAppService tagAppService)
    {
        _articleAppService = articleAppService;
        _categoryAppService = categoryAppService;
        _tagAppService = tagAppService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationHelper.DefaultPageSize)
    {
        var result = await _articleAppService.GetPagedListAsync(page, pageSize);
        ViewBag.Pagination = PaginationViewModel.From(result, "Article");
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(await BuildFormAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ArticleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await FillSelectListsAsync(model);
            return View(model);
        }

        try
        {
            var authorId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            await _articleAppService.CreateAsync(new CreateArticleDto
            {
                Title = model.Title,
                Slug = model.Slug,
                Summary = model.Summary,
                Content = model.Content,
                CoverImage = model.CoverImage,
                CategoryId = model.CategoryId,
                AuthorId = authorId,
                Status = model.Status.ToString(),
                TagIds = model.SelectedTagIds
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await FillSelectListsAsync(model);
            return View(model);
        }

        TempData["Success"] = model.Status == ArticleStatus.Draft ? "草稿保存成功" : "文章发布成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var article = await _articleAppService.GetAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        var model = await BuildFormAsync();
        model.Id = article.Id;
        model.Title = article.Title;
        model.Slug = article.Slug;
        model.Summary = article.Summary;
        model.Content = article.Content;
        model.CoverImage = article.CoverImage;
        model.CategoryId = article.CategoryId;
        model.SelectedTagIds = article.TagIds;
        model.Status = Enum.Parse<Models.Blog.ArticleStatus>(article.Status);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ArticleFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await FillSelectListsAsync(model);
            return View(model);
        }

        try
        {
            await _articleAppService.UpdateAsync(new UpdateArticleDto
            {
                Id = id,
                Title = model.Title,
                Slug = model.Slug,
                Summary = model.Summary,
                Content = model.Content,
                CoverImage = model.CoverImage,
                CategoryId = model.CategoryId,
                Status = model.Status.ToString(),
                TagIds = model.SelectedTagIds
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await FillSelectListsAsync(model);
            return View(model);
        }

        TempData["Success"] = "文章更新成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(long id)
    {
        var article = await _articleAppService.GetAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        return View(article);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _articleAppService.DeleteAsync(id);
        TempData["Success"] = "文章已删除";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ArticleFormViewModel> BuildFormAsync()
    {
        var model = new ArticleFormViewModel();
        await FillSelectListsAsync(model);
        return model;
    }

    private async Task FillSelectListsAsync(ArticleFormViewModel model)
    {
        var categories = await _categoryAppService.GetListAsync();
        model.Categories = categories
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == model.CategoryId))
            .ToList();
        model.Tags = await _tagAppService.GetListAsync();

        if (model.CategoryId == 0 && categories.Count > 0)
        {
            model.CategoryId = categories[0].Id;
        }
    }
}
