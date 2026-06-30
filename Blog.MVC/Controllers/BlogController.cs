using System.Text.Json;
using Blog.MVC.IServices.Blog;
using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.ViewModels.Site;
using Microsoft.AspNetCore.Mvc;

namespace Blog.MVC.Controllers;

public class BlogController : Controller
{
    private const string GuestCookieName = "blog_comment_guest";
    private readonly IArticleAppService _articleAppService;
    private readonly ICommentAppService _commentAppService;
    private readonly ITagAppService _tagAppService;
    private readonly ICategoryAppService _categoryAppService;

    public BlogController(
        IArticleAppService articleAppService,
        ICommentAppService commentAppService,
        ITagAppService tagAppService,
        ICategoryAppService categoryAppService)
    {
        _articleAppService = articleAppService;
        _commentAppService = commentAppService;
        _tagAppService = tagAppService;
        _categoryAppService = categoryAppService;
    }

    [HttpGet]
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

    private sealed class GuestCommentCookie
    {
        public string NickName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Website { get; set; }
    }
}
