using BeaverX.Core.Dependency;
using BeaverX.Domain.Repositories;
using Blog.MVC.Data;
using Blog.MVC.IServices.OpenSource;
using Blog.MVC.IServices.OpenSource.Dtos;
using Blog.MVC.Models.Common;
using Blog.MVC.Models.OpenSource;
using Microsoft.EntityFrameworkCore;

namespace Blog.MVC.Services.OpenSource;

public class OpenSourceProjectAppService : IOpenSourceProjectAppService, IScopedDependency
{
    private readonly IRepository<OpenSourceProject> _projectRepository;
    private readonly BlogDbContext _dbContext;

    public OpenSourceProjectAppService(IRepository<OpenSourceProject> projectRepository, BlogDbContext dbContext)
    {
        _projectRepository = projectRepository;
        _dbContext = dbContext;
    }

    public async Task<PagedResult<OpenSourceProjectDto>> GetPagedListAsync(int page = 1, int pageSize = PaginationHelper.DefaultPageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);

        var query = _dbContext.OpenSourceProjects.AsNoTracking().OrderBy(x => x.SortOrder).ThenByDescending(x => x.Id);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapToDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<OpenSourceProjectDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<OpenSourceProjectDto>> GetPublishedListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.OpenSourceProjects
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.SortOrder)
            .ThenByDescending(x => x.Id)
            .Select(x => MapToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<OpenSourceProjectDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.FindAsync(id, cancellationToken);
        return project == null ? null : MapToDto(project);
    }

    public async Task CreateAsync(CreateOpenSourceProjectDto input, CancellationToken cancellationToken = default)
    {
        var project = new OpenSourceProject
        {
            Name = input.Name.Trim(),
            Description = input.Description?.Trim(),
            RepositoryUrl = input.RepositoryUrl.Trim(),
            DemoUrl = input.DemoUrl?.Trim(),
            Language = input.Language?.Trim(),
            SortOrder = input.SortOrder,
            IsPublished = input.IsPublished
        };

        await _projectRepository.InsertAsync(project, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(UpdateOpenSourceProjectDto input, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.FindAsync(input.Id, cancellationToken)
            ?? throw new InvalidOperationException("开源项目不存在");

        project.Name = input.Name.Trim();
        project.Description = input.Description?.Trim();
        project.RepositoryUrl = input.RepositoryUrl.Trim();
        project.DemoUrl = input.DemoUrl?.Trim();
        project.Language = input.Language?.Trim();
        project.SortOrder = input.SortOrder;
        project.IsPublished = input.IsPublished;

        await _projectRepository.UpdateAsync(project, cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        _projectRepository.DeleteAsync(id, cancellationToken: cancellationToken);

    private static OpenSourceProjectDto MapToDto(OpenSourceProject x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        RepositoryUrl = x.RepositoryUrl,
        DemoUrl = x.DemoUrl,
        Language = x.Language,
        SortOrder = x.SortOrder,
        IsPublished = x.IsPublished
    };
}
