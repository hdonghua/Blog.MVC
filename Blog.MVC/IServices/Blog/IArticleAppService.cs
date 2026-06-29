using Blog.MVC.IServices.Blog.Dtos;

namespace Blog.MVC.IServices.Blog;

public interface IArticleAppService
{
    Task<List<ArticleListDto>> GetListAsync(CancellationToken cancellationToken = default);

    Task<ArticleDetailDto?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateArticleDto input, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateArticleDto input, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
