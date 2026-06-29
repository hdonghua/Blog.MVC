using BeaverX.Domain.Entities;

namespace Blog.MVC.Models.Blog;

public class Tag : FullAuditedEntity
{
    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
