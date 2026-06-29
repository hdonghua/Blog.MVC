using BeaverX.Core.Dependency;
using BeaverX.Domain.Repositories;
using Blog.MVC.Data;
using Blog.MVC.Helpers;
using Blog.MVC.IServices.Blog;
using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Models.Blog;
using Microsoft.EntityFrameworkCore;

namespace Blog.MVC.Services.Blog;

public class CategoryAppService : ICategoryAppService, IScopedDependency
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly BlogDbContext _dbContext;

    public CategoryAppService(IRepository<Category> categoryRepository, BlogDbContext dbContext)
    {
        _categoryRepository = categoryRepository;
        _dbContext = dbContext;
    }

    public async Task<List<CategoryDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetListAsync(cancellationToken: cancellationToken);
        var articleCounts = await _dbContext.Articles
            .GroupBy(x => x.CategoryId)
            .Select(x => new { CategoryId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

        return categories
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                SortOrder = x.SortOrder,
                ArticleCount = articleCounts.GetValueOrDefault(x.Id)
            })
            .ToList();
    }

    public async Task<CategoryDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.FindAsync(id, cancellationToken);
        if (category == null)
        {
            return null;
        }

        var articleCount = await _dbContext.Articles.CountAsync(x => x.CategoryId == id, cancellationToken);
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            SortOrder = category.SortOrder,
            ArticleCount = articleCount
        };
    }

    public async Task CreateAsync(CreateCategoryDto input, CancellationToken cancellationToken = default)
    {
        var slug = SlugHelper.Generate(input.Slug ?? input.Name, "category");
        if (await _categoryRepository.FindAsync(x => x.Slug == slug, cancellationToken: cancellationToken) != null)
        {
            throw new InvalidOperationException("分类别名已存在");
        }

        await _categoryRepository.InsertAsync(new Category
        {
            Name = input.Name.Trim(),
            Slug = slug,
            Description = input.Description?.Trim(),
            SortOrder = input.SortOrder
        }, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(UpdateCategoryDto input, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.FindAsync(input.Id, cancellationToken)
            ?? throw new InvalidOperationException("分类不存在");

        var slug = SlugHelper.Generate(input.Slug ?? input.Name, "category");
        var duplicate = await _categoryRepository.FindAsync(
            x => x.Slug == slug && x.Id != input.Id,
            cancellationToken: cancellationToken);
        if (duplicate != null)
        {
            throw new InvalidOperationException("分类别名已存在");
        }

        category.Name = input.Name.Trim();
        category.Slug = slug;
        category.Description = input.Description?.Trim();
        category.SortOrder = input.SortOrder;

        await _categoryRepository.UpdateAsync(category, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Articles.AnyAsync(x => x.CategoryId == id, cancellationToken))
        {
            throw new InvalidOperationException("该分类下还有文章，无法删除");
        }

        await _categoryRepository.DeleteAsync(id, cancellationToken: cancellationToken);
    }
}
