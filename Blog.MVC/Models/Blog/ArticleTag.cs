namespace Blog.MVC.Models.Blog;

public class ArticleTag
{
    public long ArticleId { get; set; }

    public Article Article { get; set; } = null!;

    public long TagId { get; set; }

    public Tag Tag { get; set; } = null!;
}
