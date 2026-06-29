using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Models.Common;

namespace Blog.MVC.IServices.Blog;

public interface ICategoryAppService
{
    Task<PagedResult<CategoryDto>> GetPagedListAsync(int page = 1, int pageSize = PaginationHelper.DefaultPageSize, CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    Task<List<CategoryDto>> GetListAsync(CancellationToken cancellationToken = default);

    Task<CategoryDto?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateCategoryDto input, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateCategoryDto input, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
