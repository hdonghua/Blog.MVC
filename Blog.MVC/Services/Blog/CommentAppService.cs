using BeaverX.Core.Dependency;
using BeaverX.Domain.Repositories;
using Blog.MVC.Data;
using Blog.MVC.IServices.Blog;
using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Models.Blog;
using Blog.MVC.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Blog.MVC.Services.Blog;

public class CommentAppService : ICommentAppService, IScopedDependency
{
    private readonly IRepository<Comment> _commentRepository;
    private readonly BlogDbContext _dbContext;

    public CommentAppService(IRepository<Comment> commentRepository, BlogDbContext dbContext)
    {
        _commentRepository = commentRepository;
        _dbContext = dbContext;
    }

    public async Task<List<CommentDto>> GetApprovedByArticleIdAsync(long articleId, CancellationToken cancellationToken = default)
    {
        var comments = await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.ArticleId == articleId && x.Status == 1)
            .OrderBy(x => x.CreationTime)
            .Select(x => new CommentDto
            {
                Id = x.Id,
                ArticleId = x.ArticleId,
                ParentId = x.ParentId,
                NickName = x.NickName,
                Website = x.Website,
                Content = x.Content,
                CreationTime = x.CreationTime
            })
            .ToListAsync(cancellationToken);

        return BuildCommentTree(comments);
    }

    public Task<int> GetApprovedCountByArticleIdAsync(long articleId, CancellationToken cancellationToken = default) =>
        _dbContext.Comments.CountAsync(
            x => x.ArticleId == articleId && x.Status == 1,
            cancellationToken);

    public Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Comments.CountAsync(x => x.Status == 0, cancellationToken);

    public async Task<PagedResult<CommentAdminDto>> GetAdminPagedListAsync(
        bool pendingOnly,
        int page = 1,
        int pageSize = PaginationHelper.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);

        var query = _dbContext.Comments
            .AsNoTracking()
            .Include(x => x.Article)
            .Where(x => pendingOnly
                ? x.Status == 0
                : x.Status == 1)
            .OrderByDescending(x => x.CreationTime);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CommentAdminDto
            {
                Id = x.Id,
                ArticleId = x.ArticleId,
                ArticleTitle = x.Article.Title,
                ArticleSlug = x.Article.Slug,
                ParentId = x.ParentId,
                NickName = x.NickName,
                Email = x.Email,
                Website = x.Website,
                Content = x.Content,
                CreationTime = x.CreationTime,
                IsApproved = x.Status == 1
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CommentAdminDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task CreateAsync(CreateCommentDto input, CancellationToken cancellationToken = default)
    {
        var articleExists = await _dbContext.Articles
            .AnyAsync(x => x.Id == input.ArticleId && x.Status == ArticleStatus.Published, cancellationToken);
        if (!articleExists)
        {
            throw new InvalidOperationException("文章不存在或未发布");
        }

        if (input.ParentId.HasValue)
        {
            var parent = await _dbContext.Comments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == input.ParentId.Value, cancellationToken)
                ?? throw new InvalidOperationException("回复的评论不存在");

            if (parent.ArticleId != input.ArticleId)
            {
                throw new InvalidOperationException("无法回复其他文章的评论");
            }

            if (parent.Status == 0)
            {
                throw new InvalidOperationException("无法回复待审核的评论");
            }
        }

        if (!string.IsNullOrWhiteSpace(input.Website) &&
            !Uri.TryCreate(input.Website, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("网站地址格式不正确");
        }

        var comment = new Comment
        {
            ArticleId = input.ArticleId,
            ParentId = input.ParentId,
            NickName = input.NickName.Trim(),
            Email = input.Email.Trim(),
            Website = string.IsNullOrWhiteSpace(input.Website) ? null : input.Website.Trim(),
            Content = input.Content.Trim()
        };

        await _commentRepository.InsertAsync(comment, cancellationToken: cancellationToken);
    }

    public async Task ApproveAsync(long id, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.FindAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("评论不存在");

        if (comment.Status == 1)
        {
            return;
        }

        if (comment.ParentId.HasValue)
        {
            var parentApproved = await _dbContext.Comments
                .AnyAsync(x => x.Id == comment.ParentId.Value && x.Status == 1, cancellationToken);
            if (!parentApproved)
            {
                throw new InvalidOperationException("请先审核通过被回复的评论");
            }
        }
        comment.Status = 1;

        await _commentRepository.UpdateAsync(comment, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _commentRepository.DeleteAsync(id, cancellationToken: cancellationToken);
    }

    private static List<CommentDto> BuildCommentTree(List<CommentDto> comments)
    {
        var lookup = comments.ToDictionary(x => x.Id);
        var roots = new List<CommentDto>();

        foreach (var comment in comments)
        {
            if (comment.ParentId is long parentId && lookup.TryGetValue(parentId, out var parent))
            {
                comment.ReplyToNickName = parent.NickName;
                parent.Replies.Add(comment);
            }
            else
            {
                roots.Add(comment);
            }
        }

        return roots;
    }
}
