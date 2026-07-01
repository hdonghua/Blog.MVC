using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Models.Common;

namespace Blog.MVC.IServices.Blog;

public interface IArticleAppService
{
    Task<PagedResult<ArticleListDto>> GetPagedListAsync(int page = 1, int pageSize = PaginationHelper.DefaultPageSize, CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    Task<List<ArticleListDto>> GetListAsync(CancellationToken cancellationToken = default);

    Task<ArticleDetailDto?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task<PagedResult<ArticleListDto>> GetPublishedPagedListAsync(int page = 1, int pageSize = PaginationHelper.DefaultPageSize, CancellationToken cancellationToken = default);

    Task<List<ArticleListDto>> GetPublishedListAsync(string? categorySlug = null, string? tagSlug = null, CancellationToken cancellationToken = default);

    Task<List<ArticleSearchResultDto>> SearchPublishedAsync(string keyword, int limit = 10, CancellationToken cancellationToken = default);

    Task<List<ArticleListDto>> SearchPublishedArticlesAsync(string keyword, int limit = 30, CancellationToken cancellationToken = default);

    Task<ArticleDetailDto?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task IncrementViewCountAsync(long id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateArticleDto input, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateArticleDto input, CancellationToken cancellationToken = default);

    Task UpdateContentAsync(long id, string content, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<List<DashboardDailyStatDto>> GetPublishedDailyTrendAsync(int days = 30, CancellationToken cancellationToken = default);

    Task<List<DashboardArticleViewDto>> GetTopViewedArticlesAsync(int top = 5, CancellationToken cancellationToken = default);
}
