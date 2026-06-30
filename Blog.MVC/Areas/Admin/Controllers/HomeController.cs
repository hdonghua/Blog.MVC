using Blog.MVC.IServices.Blog;
using Blog.MVC.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class HomeController : Controller
{
    private readonly IArticleAppService _articleAppService;
    private readonly ICategoryAppService _categoryAppService;
    private readonly ITagAppService _tagAppService;
    private readonly ICommentAppService _commentAppService;

    public HomeController(
        IArticleAppService articleAppService,
        ICategoryAppService categoryAppService,
        ITagAppService tagAppService,
        ICommentAppService commentAppService)
    {
        _articleAppService = articleAppService;
        _categoryAppService = categoryAppService;
        _tagAppService = tagAppService;
        _commentAppService = commentAppService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            ArticleCount = await _articleAppService.GetCountAsync(),
            CategoryCount = await _categoryAppService.GetCountAsync(),
            TagCount = await _tagAppService.GetCountAsync(),
            PendingCommentCount = await _commentAppService.GetPendingCountAsync(),
            PublishTrend = await _articleAppService.GetPublishedDailyTrendAsync(),
            TopViewedArticles = await _articleAppService.GetTopViewedArticlesAsync(),
            CurrentUserName = User.Identity?.Name ?? "博主"
        };

        return View(model);
    }
}
