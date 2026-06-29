using Blog.MVC.IServices.Blog.Dtos;

namespace Blog.MVC.IServices.Blog;

public interface ITagAppService
{
    Task<List<TagDto>> GetListAsync(CancellationToken cancellationToken = default);

    Task<TagDto?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateTagDto input, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateTagDto input, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
