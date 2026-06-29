using BeaverX.Domain.Entities;
using Blog.MVC.Models.Users;

namespace Blog.MVC.Models.Blog;

public class Article : FullAuditedEntity
{
    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public string Content { get; set; } = null!;

    public string? CoverImage { get; set; }

    public long CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public long? AuthorId { get; set; }

    public AppUser? Author { get; set; }

    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    public int ViewCount { get; set; }

    public DateTime? PublishedTime { get; set; }

    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
