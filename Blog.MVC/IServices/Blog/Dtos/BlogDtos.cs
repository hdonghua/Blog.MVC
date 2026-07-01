namespace Blog.MVC.IServices.Blog.Dtos;

public class CategoryDto
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public int ArticleCount { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public int SortOrder { get; set; }
}

public class UpdateCategoryDto
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public int SortOrder { get; set; }
}

public class TagDto
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int ArticleCount { get; set; }
}

public class CreateTagDto
{
    public string Name { get; set; } = null!;

    public string? Slug { get; set; }
}

public class UpdateTagDto
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Slug { get; set; }
}

public class ArticleTagBriefDto
{
    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;
}

public class ArticleSearchResultDto
{
    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public string CategoryName { get; set; } = null!;
}

public class ArticleListDto
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? CoverImage { get; set; }

    public string? Summary { get; set; }

    public string CategoryName { get; set; } = null!;

    public string CategorySlug { get; set; } = null!;

    public string? AuthorName { get; set; }

    public string Status { get; set; } = null!;

    public int ViewCount { get; set; }

    public int CommentCount { get; set; }

    public DateTime? PublishedTime { get; set; }

    public DateTime CreationTime { get; set; }

    public List<ArticleTagBriefDto> Tags { get; set; } = [];
}

public class ArticleDetailDto
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public string Content { get; set; } = null!;

    public string? CoverImage { get; set; }

    public long CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string CategorySlug { get; set; } = null!;

    public long? AuthorId { get; set; }

    public string? AuthorName { get; set; }

    public string Status { get; set; } = null!;

    public int ViewCount { get; set; }

    public DateTime? PublishedTime { get; set; }

    public List<long> TagIds { get; set; } = [];

    public List<ArticleTagBriefDto> Tags { get; set; } = [];
}

public class CreateArticleDto
{
    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string Content { get; set; } = null!;

    public string? CoverImage { get; set; }

    public long CategoryId { get; set; }

    public long? AuthorId { get; set; }

    public string Status { get; set; } = null!;

    public List<long> TagIds { get; set; } = [];
}

public class UpdateArticleDto
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string Content { get; set; } = null!;

    public string? CoverImage { get; set; }

    public long CategoryId { get; set; }

    public string Status { get; set; } = null!;

    public List<long> TagIds { get; set; } = [];
}

public class AutoSaveArticleContentDto
{
    public long Id { get; set; }

    public string Content { get; set; } = null!;
}

public class DashboardDailyStatDto
{
    public string Label { get; set; } = null!;

    public int Count { get; set; }
}

public class DashboardArticleViewDto
{
    public string Title { get; set; } = null!;

    public int ViewCount { get; set; }
}
