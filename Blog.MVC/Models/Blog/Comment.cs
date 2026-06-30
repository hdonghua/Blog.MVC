using BeaverX.Domain.Entities;

namespace Blog.MVC.Models.Blog;

public class Comment : AuditedEntity
{
    public long ArticleId { get; set; }

    public Article Article { get; set; } = null!;

    public long? ParentId { get; set; }

    public Comment? Parent { get; set; }

    public string NickName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Website { get; set; }

    public string Content { get; set; } = null!;

    public int Status { get; set; }
}
