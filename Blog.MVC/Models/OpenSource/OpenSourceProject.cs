using BeaverX.Domain.Entities;

namespace Blog.MVC.Models.OpenSource;

public class OpenSourceProject : FullAuditedEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string RepositoryUrl { get; set; } = null!;

    public string? DemoUrl { get; set; }

    public string? Language { get; set; }

    public int SortOrder { get; set; }

    public bool IsPublished { get; set; } = true;
}
