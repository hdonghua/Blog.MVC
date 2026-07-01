using System.Text.Json;
using Blog.MVC.IServices.Blog;
using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Services.Site;
using Blog.MVC.ViewModels.Site;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Controllers;

public class BlogController : Controller
{
    private const string GuestCookieName = "blog_comment_guest";
    private const int RssItemLimit = 50;
    private readonly IArticleAppService _articleAppService;
    private readonly ICommentAppService _commentAppService;
    private readonly ITagAppService _tagAppService;
    private readonly ICategoryAppService _categoryAppService;
    private readonly IConfiguration _configuration;

    public BlogController(
        IArticleAppService articleAppService,
        ICommentAppService commentAppService,
        ITagAppService tagAppService,
        ICategoryAppService categoryAppService,
        IConfiguration configuration)
    {
        _articleAppService = articleAppService;
        _commentAppService = commentAppService;
        _tagAppService = tagAppService;
        _categoryAppService = categoryAppService;
        _configuration = configuration;
    }

    [HttpGet("/Blog/Detail/{slug}")]
    public async Task<IActionResult> Detail(string slug, long? replyTo, CancellationToken cancellationToken)
    {
        var article = await _articleAppService.GetPublishedBySlugAsync(slug, cancellationToken);
        if (article == null)
        {
            return NotFound();
        }

        await _articleAppService.IncrementViewCountAsync(article.Id, cancellationToken);

        var comments = await _commentAppService.GetApprovedByArticleIdAsync(article.Id, cancellationToken);
        var commentCount = await _commentAppService.GetApprovedCountByArticleIdAsync(article.Id, cancellationToken);
        var guest = ReadGuestCookie();

        string? replyToNickName = null;
        if (replyTo.HasValue)
        {
            replyToNickName = FindCommentNickName(comments, replyTo.Value);
        }

        return View(new BlogDetailViewModel
        {
            Article = article,
            Comments = comments,
            CommentCount = commentCount,
            CommentForm = new PostCommentViewModel
            {
                ArticleId = article.Id,
                Slug = article.Slug,
                ParentId = replyToNickName != null ? replyTo : null,
                ReplyToNickName = replyToNickName,
                NickName = guest?.NickName ?? string.Empty,
                Email = guest?.Email ?? string.Empty,
                Website = guest?.Website
            }
        });
    }

    [HttpGet("/rss")]
    public async Task<IActionResult> Rss(CancellationToken cancellationToken)
    {
        var result = await _articleAppService.GetPublishedPagedListAsync(1, RssItemLimit, cancellationToken);
        var siteTitle = _configuration["Site:Title"] ?? "HBLOG";
        var siteDescription = _configuration["Site:Description"] ?? "个人博客";
        var siteUrl = ResolveSiteBaseUrl();
        var feedUrl = $"{siteUrl}/rss";
        var managingEditor = ResolveManagingEditor(siteTitle);

        var feedBytes = RssFeedBuilder.Build(
            siteTitle,
            siteDescription,
            siteUrl,
            feedUrl,
            managingEditor,
            result.Items,
            slug => $"{siteUrl}/Blog/Detail/{slug}");

        return File(feedBytes, "application/rss+xml; charset=utf-8");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostComment([Bind(Prefix = "CommentForm")] PostCommentViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Website))
        {
            model.Website = null;
        }

        var article = await _articleAppService.GetPublishedBySlugAsync(model.Slug, cancellationToken);
        if (article == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return await RenderDetailWithFormErrors(article, model, cancellationToken);
        }

        try
        {
            await _commentAppService.CreateAsync(new CreateCommentDto
            {
                ArticleId = article.Id,
                ParentId = model.ParentId,
                NickName = model.NickName,
                Email = model.Email,
                Website = model.Website,
                Content = model.Content
            }, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await RenderDetailWithFormErrors(article, model, cancellationToken);
        }

        SaveGuestCookie(model.NickName, model.Email, model.Website);
        TempData["CommentPending"] = true;

        return RedirectToAction(nameof(Detail), new { slug = model.Slug, replyTo = (long?)null });
    }

    private async Task<IActionResult> RenderDetailWithFormErrors(
        ArticleDetailDto article,
        PostCommentViewModel model,
        CancellationToken cancellationToken)
    {
        var comments = await _commentAppService.GetApprovedByArticleIdAsync(article.Id, cancellationToken);
        var commentCount = await _commentAppService.GetApprovedCountByArticleIdAsync(article.Id, cancellationToken);

        return View("Detail", new BlogDetailViewModel
        {
            Article = article,
            Comments = comments,
            CommentCount = commentCount,
            CommentForm = model
        });
    }

    [HttpGet("/api/blog/search")]
    public async Task<IActionResult> SearchApi([FromQuery] string? q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Json(Array.Empty<ArticleSearchResultDto>());
        }

        var results = await _articleAppService.SearchPublishedAsync(q, 8, cancellationToken);
        return Json(results, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = q?.Trim() ?? string.Empty;
        var results = string.IsNullOrEmpty(query)
            ? []
            : await _articleAppService.SearchPublishedArticlesAsync(query, cancellationToken: cancellationToken);

        return View(new SearchViewModel
        {
            Query = query,
            Results = results
        });
    }

    [HttpGet]
    public async Task<IActionResult> Tag(string slug, CancellationToken cancellationToken)
    {
        var tag = await _tagAppService.GetBySlugAsync(slug, cancellationToken);
        if (tag == null)
        {
            return NotFound();
        }

        var articles = await _articleAppService.GetPublishedListAsync(tagSlug: slug, cancellationToken: cancellationToken);
        return View(new TagArticlesViewModel
        {
            Tag = tag,
            Articles = articles
        });
    }

    [HttpGet]
    public async Task<IActionResult> Timeline(string? category, string? tag, CancellationToken cancellationToken)
    {
        CategoryDto? activeCategory = null;
        TagDto? activeTag = null;

        if (!string.IsNullOrWhiteSpace(category))
        {
            activeCategory = await _categoryAppService.GetBySlugAsync(category, cancellationToken);
            if (activeCategory == null)
            {
                return NotFound();
            }
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            activeTag = await _tagAppService.GetBySlugAsync(tag, cancellationToken);
            if (activeTag == null)
            {
                return NotFound();
            }
        }

        var articles = await _articleAppService.GetPublishedListAsync(category, tag, cancellationToken);
        var groups = articles
            .GroupBy(x => (x.PublishedTime ?? x.CreationTime).ToLocalTime().ToString("yyyy年M月"))
            .Select(g => new TimelineGroupViewModel
            {
                Label = g.Key,
                Articles = g.ToList()
            })
            .ToList();

        return View(new TimelineViewModel
        {
            Groups = groups,
            Categories = await _categoryAppService.GetPublishedListAsync(cancellationToken),
            Tags = await _tagAppService.GetPublishedListAsync(cancellationToken),
            ActiveCategorySlug = activeCategory?.Slug,
            ActiveCategoryName = activeCategory?.Name,
            ActiveTagSlug = activeTag?.Slug,
            ActiveTagName = activeTag?.Name
        });
    }

    private static string? FindCommentNickName(IEnumerable<CommentDto> comments, long commentId)
    {
        foreach (var comment in comments)
        {
            if (comment.Id == commentId)
            {
                return comment.NickName;
            }

            var nested = FindCommentNickName(comment.Replies, commentId);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private void SaveGuestCookie(string nickName, string email, string? website)
    {
        var payload = JsonSerializer.Serialize(new GuestCommentCookie
        {
            NickName = nickName,
            Email = email,
            Website = website
        });

        Response.Cookies.Append(GuestCookieName, payload, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });
    }

    private GuestCommentCookie? ReadGuestCookie()
    {
        if (!Request.Cookies.TryGetValue(GuestCookieName, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<GuestCommentCookie>(value);
        }
        catch
        {
            return null;
        }
    }

    private string ResolveSiteBaseUrl()
    {
        var configured = _configuration["Site:BaseUrl"]?.Trim().TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        return $"{Request.Scheme}://{Request.Host}";
    }

    private string? ResolveManagingEditor(string siteTitle)
    {
        var configured = _configuration["Site:ManagingEditor"]?.Trim();
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        var email = _configuration["Site:Email"]?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return $"{email} ({siteTitle})";
    }

    private sealed class GuestCommentCookie
    {
        public string NickName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Website { get; set; }
    }
}
