using Blog.MVC.Helpers;
using Blog.MVC.IServices.Blog;
using Blog.MVC.ViewModels.Site;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Controllers;

public class BlogController : Controller
{
    private readonly IArticleAppService _articleAppService;

    public BlogController(IArticleAppService articleAppService)
    {
        _articleAppService = articleAppService;
    }

    [HttpGet]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        var article = await _articleAppService.GetPublishedBySlugAsync(slug, cancellationToken);
        if (article == null)
        {
            return NotFound();
        }

        await _articleAppService.IncrementViewCountAsync(article.Id, cancellationToken);

        return View(new BlogDetailViewModel
        {
            Article = article,
            HtmlContent = MarkdownHelper.ToHtml(article.Content)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Timeline(CancellationToken cancellationToken)
    {
        var articles = await _articleAppService.GetPublishedListAsync(cancellationToken);
        var groups = articles
            .GroupBy(x => (x.PublishedTime ?? x.CreationTime).ToLocalTime().ToString("yyyy年M月"))
            .Select(g => new TimelineGroupViewModel
            {
                Label = g.Key,
                Articles = g.ToList()
            })
            .ToList();

        return View(new TimelineViewModel { Groups = groups });
    }
}
