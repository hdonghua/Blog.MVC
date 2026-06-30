namespace Blog.MVC.IServices.OpenSource.Dtos;

public class OpenSourceProjectDto
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string RepositoryUrl { get; set; } = null!;

    public string? DemoUrl { get; set; }

    public string? Language { get; set; }

    public int SortOrder { get; set; }

    public bool IsPublished { get; set; }
}

public class CreateOpenSourceProjectDto
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string RepositoryUrl { get; set; } = null!;

    public string? DemoUrl { get; set; }

    public string? Language { get; set; }

    public int SortOrder { get; set; }

    public bool IsPublished { get; set; } = true;
}

public class UpdateOpenSourceProjectDto
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string RepositoryUrl { get; set; } = null!;

    public string? DemoUrl { get; set; }

    public string? Language { get; set; }

    public int SortOrder { get; set; }

    public bool IsPublished { get; set; } = true;
}
