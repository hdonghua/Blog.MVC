using Blog.MVC.Helpers;
using Blog.MVC.IServices.Blog;
using Blog.MVC.IServices.OpenSource;
using Blog.MVC.ViewModels.Site;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using Blog.MVC.Models;

namespace Blog.MVC.Controllers;

public class HomeController : Controller
{
    private readonly IArticleAppService _articleAppService;
    private readonly IOpenSourceProjectAppService _openSourceProjectAppService;
    private readonly IConfiguration _configuration;

    public HomeController(
        IArticleAppService articleAppService,
        IOpenSourceProjectAppService openSourceProjectAppService,
        IConfiguration configuration)
    {
        _articleAppService = articleAppService;
        _openSourceProjectAppService = openSourceProjectAppService;
        _configuration = configuration;
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

    [HttpGet("/robots.txt")]
    public ContentResult Robots()
    {
        var baseUrl = SeoHelper.ResolveBaseUrl(_configuration, Request);
        var content = new StringBuilder()
            .AppendLine("User-agent: *")
            .AppendLine("Allow: /")
            .AppendLine("Disallow: /Admin/")
            .AppendLine()
            .Append("Sitemap: ")
            .Append(baseUrl)
            .AppendLine("/sitemap.xml")
            .ToString();

        return Content(content, "text/plain", Encoding.UTF8);
    }

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Sitemap(CancellationToken cancellationToken)
    {
        var articles = await _articleAppService.GetPublishedListAsync(cancellationToken: cancellationToken);
        var baseUrl = SeoHelper.ResolveBaseUrl(_configuration, Request);
        var xml = SeoHelper.BuildSitemap(baseUrl, articles);
        return File(xml, "application/xml; charset=utf-8");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
