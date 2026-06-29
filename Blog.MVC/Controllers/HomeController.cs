using Blog.MVC.IServices.Blog;
using Blog.MVC.ViewModels.Site;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Blog.MVC.Models;

namespace Blog.MVC.Controllers;

public class HomeController : Controller
{
    private readonly IArticleAppService _articleAppService;

    public HomeController(IArticleAppService articleAppService)
    {
        _articleAppService = articleAppService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _articleAppService.GetPublishedPagedListAsync(1, 6, cancellationToken);
        return View(new HomeIndexViewModel { RecentArticles = result.Items });
    }

    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
