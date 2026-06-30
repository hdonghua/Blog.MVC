using BeaverX.Core.Dependency;
using BeaverX.Domain.Repositories;
using Blog.MVC.Data;
using Blog.MVC.Helpers;
using Blog.MVC.IServices.Blog;
using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Models.Blog;
using Blog.MVC.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Blog.MVC.Services.Blog;

public class ArticleAppService : IArticleAppService, IScopedDependency
{
    private readonly IRepository<Article> _articleRepository;
    private readonly BlogDbContext _dbContext;

    public ArticleAppService(IRepository<Article> articleRepository, BlogDbContext dbContext)
    {
        _articleRepository = articleRepository;
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ArticleListDto>> GetPagedListAsync(int page = 1, int pageSize = PaginationHelper.DefaultPageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);

        var query = _dbContext.Articles.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .OrderByDescending(x => x.CreationTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ArticleListDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                CoverImage = x.CoverImage,
                CategoryName = x.Category.Name,
                AuthorName = x.Author != null ? (x.Author.DisplayName ?? x.Author.UserName) : null,
                Status = x.Status.ToString(),
                ViewCount = x.ViewCount,
                PublishedTime = x.PublishedTime,
                CreationTime = x.CreationTime,
                Tags = x.ArticleTags.Select(t => new ArticleTagBriefDto { Name = t.Tag.Name, Slug = t.Tag.Slug }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Articles.CountAsync(cancellationToken);

    public async Task<List<ArticleListDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Articles
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .OrderByDescending(x => x.CreationTime)
            .Select(x => new ArticleListDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                CoverImage = x.CoverImage,
                CategoryName = x.Category.Name,
                AuthorName = x.Author != null ? (x.Author.DisplayName ?? x.Author.UserName) : null,
                Status = x.Status.ToString(),
                ViewCount = x.ViewCount,
                PublishedTime = x.PublishedTime,
                CreationTime = x.CreationTime,
                Tags = x.ArticleTags.Select(t => new ArticleTagBriefDto { Name = t.Tag.Name, Slug = t.Tag.Slug }).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ArticleDetailDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var article = await _dbContext.Articles
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (article == null)
        {
            return null;
        }

        return new ArticleDetailDto
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            Content = article.Content,
            CoverImage = article.CoverImage,
            CategoryId = article.CategoryId,
            CategoryName = article.Category.Name,
            AuthorId = article.AuthorId,
            AuthorName = article.Author != null ? (article.Author.DisplayName ?? article.Author.UserName) : null,
            Status = article.Status.ToString(),
            ViewCount = article.ViewCount,
            PublishedTime = article.PublishedTime,
            TagIds = article.ArticleTags.Select(x => x.TagId).ToList(),
            Tags = article.ArticleTags.Select(x => new ArticleTagBriefDto { Name = x.Tag!.Name, Slug = x.Tag.Slug }).ToList()
        };
    }

    public async Task<PagedResult<ArticleListDto>> GetPublishedPagedListAsync(int page = 1, int pageSize = PaginationHelper.DefaultPageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);

        var query = PublishedArticlesQuery();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.PublishedTime ?? x.CreationTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapToListDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<ArticleListDto>> GetPublishedListAsync(CancellationToken cancellationToken = default)
    {
        return await PublishedArticlesQuery()
            .OrderByDescending(x => x.PublishedTime ?? x.CreationTime)
            .Select(x => MapToListDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ArticleListDto>> GetPublishedListByTagSlugAsync(string tagSlug, CancellationToken cancellationToken = default)
    {
        return await PublishedArticlesQuery()
            .Where(x => x.ArticleTags.Any(at => at.Tag!.Slug == tagSlug))
            .OrderByDescending(x => x.PublishedTime ?? x.CreationTime)
            .Select(x => MapToListDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<ArticleDetailDto?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var article = await _dbContext.Articles
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.Status == ArticleStatus.Published, cancellationToken);

        if (article == null)
        {
            return null;
        }

        return new ArticleDetailDto
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            Content = article.Content,
            CoverImage = article.CoverImage,
            CategoryId = article.CategoryId,
            CategoryName = article.Category.Name,
            AuthorId = article.AuthorId,
            AuthorName = article.Author != null ? (article.Author.DisplayName ?? article.Author.UserName) : null,
            Status = article.Status.ToString(),
            ViewCount = article.ViewCount,
            PublishedTime = article.PublishedTime,
            TagIds = article.ArticleTags.Select(x => x.TagId).ToList(),
            Tags = article.ArticleTags.Select(x => new ArticleTagBriefDto { Name = x.Tag!.Name, Slug = x.Tag.Slug }).ToList()
        };
    }

    public async Task IncrementViewCountAsync(long id, CancellationToken cancellationToken = default)
    {
        var article = await _dbContext.Articles.FindAsync(id, cancellationToken);
        if (article == null)
        {
            return;
        }

        article.ViewCount++;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateAsync(CreateArticleDto input, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ArticleStatus>(input.Status, out var status))
        {
            throw new InvalidOperationException("无效的文章状态");
        }

        if (await _dbContext.Categories.FindAsync([input.CategoryId], cancellationToken) == null)
        {
            throw new InvalidOperationException("分类不存在");
        }

        var slug = await EnsureUniqueArticleSlugAsync(input.Slug ?? input.Title, null, cancellationToken);
        var tagIds = await ValidateTagIdsAsync(input.TagIds, cancellationToken);

        var article = new Article
        {
            Title = input.Title.Trim(),
            Slug = slug,
            Summary = input.Summary?.Trim(),
            Content = input.Content,
            CoverImage = input.CoverImage?.Trim(),
            CategoryId = input.CategoryId,
            AuthorId = input.AuthorId,
            Status = status,
            PublishedTime = status == ArticleStatus.Published ? DateTime.UtcNow : null
        };

        await _articleRepository.InsertAsync(article, cancellationToken: cancellationToken);
        await ReplaceArticleTagsAsync(article.Id, tagIds, cancellationToken);
    }

    public async Task UpdateAsync(UpdateArticleDto input, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ArticleStatus>(input.Status, out var status))
        {
            throw new InvalidOperationException("无效的文章状态");
        }

        var article = await _articleRepository.FindAsync(input.Id, cancellationToken)
            ?? throw new InvalidOperationException("文章不存在");

        if (await _dbContext.Categories.FindAsync([input.CategoryId], cancellationToken) == null)
        {
            throw new InvalidOperationException("分类不存在");
        }

        var slug = await EnsureUniqueArticleSlugAsync(input.Slug ?? input.Title, input.Id, cancellationToken);
        var tagIds = await ValidateTagIdsAsync(input.TagIds, cancellationToken);

        article.Title = input.Title.Trim();
        article.Slug = slug;
        article.Summary = input.Summary?.Trim();
        article.Content = input.Content;
        article.CoverImage = input.CoverImage?.Trim();
        article.CategoryId = input.CategoryId;
        article.Status = status;
        if (status == ArticleStatus.Published && article.PublishedTime == null)
        {
            article.PublishedTime = DateTime.UtcNow;
        }

        await _articleRepository.UpdateAsync(article, cancellationToken: cancellationToken);
        await ReplaceArticleTagsAsync(article.Id, tagIds, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var links = await _dbContext.ArticleTags
            .Where(x => x.ArticleId == id)
            .ToListAsync(cancellationToken);
        _dbContext.ArticleTags.RemoveRange(links);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _articleRepository.DeleteAsync(id, cancellationToken: cancellationToken);
    }

    private IQueryable<Article> PublishedArticlesQuery() =>
        _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.Status == ArticleStatus.Published)
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.Comments)
            .Include(x => x.ArticleTags)
                .ThenInclude(x => x.Tag);

    private static ArticleListDto MapToListDto(Article x) => new()
    {
        Id = x.Id,
        Title = x.Title,
        Slug = x.Slug,
        CoverImage = x.CoverImage,
        Summary = x.Summary,
        CategoryName = x.Category.Name,
        AuthorName = x.Author != null ? (x.Author.DisplayName ?? x.Author.UserName) : null,
        Status = x.Status.ToString(),
        ViewCount = x.ViewCount,
        CommentCount = x.Comments.Count(c => c.Status == 1),
        PublishedTime = x.PublishedTime,
        CreationTime = x.CreationTime,
        Tags = x.ArticleTags.Select(t => new ArticleTagBriefDto { Name = t.Tag!.Name, Slug = t.Tag.Slug }).ToList()
    };

    private async Task<string> EnsureUniqueArticleSlugAsync(
        string source,
        long? excludeId,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Generate(source, "article");
        var slug = baseSlug;
        var index = 1;

        while (true)
        {
            var exists = await _articleRepository.FindAsync(
                x => x.Slug == slug && (!excludeId.HasValue || x.Id != excludeId.Value),
                cancellationToken: cancellationToken);
            if (exists == null)
            {
                return slug;
            }

            slug = $"{baseSlug}-{index++}";
        }
    }

    private async Task<List<long>> ValidateTagIdsAsync(List<long> tagIds, CancellationToken cancellationToken)
    {
        var distinctIds = tagIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return distinctIds;
        }

        var existingCount = await _dbContext.Tags.CountAsync(x => distinctIds.Contains(x.Id), cancellationToken);
        if (existingCount != distinctIds.Count)
        {
            throw new InvalidOperationException("存在无效的标签");
        }

        return distinctIds;
    }

    private async Task ReplaceArticleTagsAsync(long articleId, List<long> tagIds, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ArticleTags
            .Where(x => x.ArticleId == articleId)
            .ToListAsync(cancellationToken);
        _dbContext.ArticleTags.RemoveRange(existing);

        foreach (var tagId in tagIds)
        {
            _dbContext.ArticleTags.Add(new ArticleTag
            {
                ArticleId = articleId,
                TagId = tagId
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<DashboardDailyStatDto>> GetPublishedDailyTrendAsync(
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (days < 1)
        {
            days = 30;
        }

        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-(days - 1));

        var counts = await _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.Status == ArticleStatus.Published && x.PublishedTime >= startDate)
            .GroupBy(x => x.PublishedTime!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var lookup = counts.ToDictionary(x => x.Date, x => x.Count);
        var result = new List<DashboardDailyStatDto>(days);

        for (var i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            lookup.TryGetValue(date, out var count);
            result.Add(new DashboardDailyStatDto
            {
                Label = date.ToString("MM-dd"),
                Count = count
            });
        }

        return result;
    }

    public async Task<List<DashboardArticleViewDto>> GetTopViewedArticlesAsync(
        int top = 5,
        CancellationToken cancellationToken = default)
    {
        if (top < 1)
        {
            top = 5;
        }

        return await _dbContext.Articles
            .AsNoTracking()
            .Where(x => x.Status == ArticleStatus.Published)
            .OrderByDescending(x => x.ViewCount)
            .ThenByDescending(x => x.PublishedTime ?? x.CreationTime)
            .Take(top)
            .Select(x => new DashboardArticleViewDto
            {
                Title = x.Title,
                ViewCount = x.ViewCount
            })
            .ToListAsync(cancellationToken);
    }
}
