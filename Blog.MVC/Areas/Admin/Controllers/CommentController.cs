using Blog.MVC.IServices.Blog;
using Blog.MVC.Models.Common;
using Blog.MVC.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CommentController : Controller
{
    private readonly ICommentAppService _commentAppService;

    public CommentController(ICommentAppService commentAppService)
    {
        _commentAppService = commentAppService;
    }

    public async Task<IActionResult> Index(bool pending = true, int page = 1, int pageSize = PaginationHelper.DefaultPageSize)
    {
        var result = await _commentAppService.GetAdminPagedListAsync(pending, page, pageSize);
        ViewBag.Pagination = PaginationViewModel.From(
            result,
            "Comment",
            routeValues: new Dictionary<string, object?> { ["pending"] = pending });
        ViewBag.PendingOnly = pending;
        ViewBag.PendingCount = await _commentAppService.GetPendingCountAsync();
        return View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long id, bool pending = true, int page = 1)
    {
        try
        {
            await _commentAppService.ApproveAsync(id);
            TempData["Success"] = "评论已通过审核。";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { pending, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, bool pending = true, int page = 1)
    {
        try
        {
            await _commentAppService.DeleteAsync(id);
            TempData["Success"] = "评论已删除。";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { pending, page });
    }
}
