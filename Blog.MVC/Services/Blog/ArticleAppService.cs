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
                CategoryName = x.Category.Name,
                AuthorName = x.Author != null ? (x.Author.DisplayName ?? x.Author.UserName) : null,
                Status = x.Status.ToString(),
                ViewCount = x.ViewCount,
                PublishedTime = x.PublishedTime,
                CreationTime = x.CreationTime,
                Tags = x.ArticleTags.Select(t => t.Tag.Name).ToList()
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
                CategoryName = x.Category.Name,
                AuthorName = x.Author != null ? (x.Author.DisplayName ?? x.Author.UserName) : null,
                Status = x.Status.ToString(),
                ViewCount = x.ViewCount,
                PublishedTime = x.PublishedTime,
                CreationTime = x.CreationTime,
                Tags = x.ArticleTags.Select(t => t.Tag.Name).ToList()
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
            TagIds = article.ArticleTags.Select(x => x.TagId).ToList()
        };
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
}
