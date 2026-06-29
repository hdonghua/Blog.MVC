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

public class TagAppService : ITagAppService, IScopedDependency
{
    private readonly IRepository<Tag> _tagRepository;
    private readonly BlogDbContext _dbContext;

    public TagAppService(IRepository<Tag> tagRepository, BlogDbContext dbContext)
    {
        _tagRepository = tagRepository;
        _dbContext = dbContext;
    }

    public async Task<PagedResult<TagDto>> GetPagedListAsync(int page = 1, int pageSize = PaginationHelper.DefaultPageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);

        var query = _dbContext.Tags.AsNoTracking().OrderBy(x => x.Name);
        var totalCount = await query.CountAsync(cancellationToken);
        var tags = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var tagIds = tags.Select(x => x.Id).ToList();
        var articleCounts = await _dbContext.ArticleTags
            .Where(x => tagIds.Contains(x.TagId))
            .GroupBy(x => x.TagId)
            .Select(x => new { TagId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.TagId, x => x.Count, cancellationToken);

        return new PagedResult<TagDto>
        {
            Items = tags.Select(x => new TagDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                ArticleCount = articleCounts.GetValueOrDefault(x.Id)
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Tags.CountAsync(cancellationToken);

    public async Task<List<TagDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetListAsync(cancellationToken: cancellationToken);
        var articleCounts = await _dbContext.ArticleTags
            .GroupBy(x => x.TagId)
            .Select(x => new { TagId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.TagId, x => x.Count, cancellationToken);

        return tags
            .OrderBy(x => x.Name)
            .Select(x => new TagDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                ArticleCount = articleCounts.GetValueOrDefault(x.Id)
            })
            .ToList();
    }

    public async Task<TagDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.FindAsync(id, cancellationToken);
        if (tag == null)
        {
            return null;
        }

        var articleCount = await _dbContext.ArticleTags.CountAsync(x => x.TagId == id, cancellationToken);
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug,
            ArticleCount = articleCount
        };
    }

    public async Task CreateAsync(CreateTagDto input, CancellationToken cancellationToken = default)
    {
        var slug = SlugHelper.Generate(input.Slug ?? input.Name, "tag");
        if (await _tagRepository.FindAsync(x => x.Slug == slug, cancellationToken: cancellationToken) != null)
        {
            throw new InvalidOperationException("标签别名已存在");
        }

        await _tagRepository.InsertAsync(new Tag
        {
            Name = input.Name.Trim(),
            Slug = slug
        }, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(UpdateTagDto input, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.FindAsync(input.Id, cancellationToken)
            ?? throw new InvalidOperationException("标签不存在");

        var slug = SlugHelper.Generate(input.Slug ?? input.Name, "tag");
        var duplicate = await _tagRepository.FindAsync(
            x => x.Slug == slug && x.Id != input.Id,
            cancellationToken: cancellationToken);
        if (duplicate != null)
        {
            throw new InvalidOperationException("标签别名已存在");
        }

        tag.Name = input.Name.Trim();
        tag.Slug = slug;

        await _tagRepository.UpdateAsync(tag, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.ArticleTags.AnyAsync(x => x.TagId == id, cancellationToken))
        {
            throw new InvalidOperationException("该标签已被文章使用，无法删除");
        }

        await _tagRepository.DeleteAsync(id, cancellationToken: cancellationToken);
    }
}
