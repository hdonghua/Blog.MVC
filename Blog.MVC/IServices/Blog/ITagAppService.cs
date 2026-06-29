using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Models.Common;

namespace Blog.MVC.IServices.Blog;

public interface ITagAppService
{
    Task<PagedResult<TagDto>> GetPagedListAsync(int page = 1, int pageSize = PaginationHelper.DefaultPageSize, CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    Task<List<TagDto>> GetListAsync(CancellationToken cancellationToken = default);

    Task<TagDto?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateTagDto input, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateTagDto input, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
