using Blog.MVC.IServices.Blog;
using Blog.MVC.IServices.Users;
using Blog.MVC.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class HomeController : Controller
{
    private readonly IUserAppService _userAppService;
    private readonly IArticleAppService _articleAppService;
    private readonly ICategoryAppService _categoryAppService;
    private readonly ITagAppService _tagAppService;

    public HomeController(
        IUserAppService userAppService,
        IArticleAppService articleAppService,
        ICategoryAppService categoryAppService,
        ITagAppService tagAppService)
    {
        _userAppService = userAppService;
        _articleAppService = articleAppService;
        _categoryAppService = categoryAppService;
        _tagAppService = tagAppService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userAppService.GetListAsync();
        var articles = await _articleAppService.GetListAsync();
        var categories = await _categoryAppService.GetListAsync();
        var tags = await _tagAppService.GetListAsync();

        var model = new DashboardViewModel
        {
            UserCount = users.Count,
            ArticleCount = articles.Count,
            CategoryCount = categories.Count,
            TagCount = tags.Count,
            CurrentUserName = User.Identity?.Name ?? "管理员"
        };

        return View(model);
    }
}
