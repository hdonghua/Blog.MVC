using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Models.Common;

namespace Blog.MVC.IServices.Blog;

public interface ICommentAppService
{
    Task<List<CommentDto>> GetApprovedByArticleIdAsync(long articleId, CancellationToken cancellationToken = default);

    Task<int> GetApprovedCountByArticleIdAsync(long articleId, CancellationToken cancellationToken = default);

    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<CommentAdminDto>> GetAdminPagedListAsync(
        bool pendingOnly,
        int page = 1,
        int pageSize = PaginationHelper.DefaultPageSize,
        CancellationToken cancellationToken = default);

    Task CreateAsync(CreateCommentDto input, CancellationToken cancellationToken = default);

    Task ApproveAsync(long id, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
