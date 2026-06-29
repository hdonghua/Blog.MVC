using Blog.MVC.IServices.Blog.Dtos;

namespace Blog.MVC.ViewModels.Site;

public class HomeIndexViewModel
{
    public List<ArticleListDto> RecentArticles { get; set; } = [];
}

public class TimelineViewModel
{
    public List<TimelineGroupViewModel> Groups { get; set; } = [];
}

public class TimelineGroupViewModel
{
    public string Label { get; set; } = null!;

    public List<ArticleListDto> Articles { get; set; } = [];
}

public class BlogDetailViewModel
{
    public ArticleDetailDto Article { get; set; } = null!;

    public string HtmlContent { get; set; } = null!;
}
