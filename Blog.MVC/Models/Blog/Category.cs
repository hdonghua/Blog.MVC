using BeaverX.Domain.Entities;

namespace Blog.MVC.Models.Blog;

public class Category : FullAuditedEntity
{
    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
