using Blog.MVC.IServices.Blog.Dtos;

namespace Blog.MVC.IServices.Blog;

public interface ICategoryAppService
{
    Task<List<CategoryDto>> GetListAsync(CancellationToken cancellationToken = default);

    Task<CategoryDto?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateCategoryDto input, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateCategoryDto input, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
