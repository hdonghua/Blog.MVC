using Blog.MVC.IServices.OpenSource.Dtos;
using Blog.MVC.Models.Common;

namespace Blog.MVC.IServices.OpenSource;

public interface IOpenSourceProjectAppService
{
    Task<PagedResult<OpenSourceProjectDto>> GetPagedListAsync(int page = 1, int pageSize = PaginationHelper.DefaultPageSize, CancellationToken cancellationToken = default);

    Task<List<OpenSourceProjectDto>> GetPublishedListAsync(CancellationToken cancellationToken = default);

    Task<OpenSourceProjectDto?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateOpenSourceProjectDto input, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateOpenSourceProjectDto input, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
