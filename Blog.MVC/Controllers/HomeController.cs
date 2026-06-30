using Blog.MVC.IServices.Blog;
using Blog.MVC.IServices.OpenSource;
using Blog.MVC.ViewModels.Site;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Blog.MVC.Models;

namespace Blog.MVC.Controllers;

public class HomeController : Controller
{
    private readonly IArticleAppService _articleAppService;
    private readonly IOpenSourceProjectAppService _openSourceProjectAppService;

    public HomeController(
        IArticleAppService articleAppService,
        IOpenSourceProjectAppService openSourceProjectAppService)
    {
        _articleAppService = articleAppService;
        _openSourceProjectAppService = openSourceProjectAppService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _articleAppService.GetPublishedPagedListAsync(1, 6, cancellationToken);
        var projects = await _openSourceProjectAppService.GetPublishedListAsync(cancellationToken);
        return View(new HomeIndexViewModel
        {
            RecentArticles = result.Items,
            OpenSourceProjects = projects
        });
    }

    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
