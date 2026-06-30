using System.ComponentModel.DataAnnotations;
using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.IServices.OpenSource.Dtos;

namespace Blog.MVC.ViewModels.Site;
public class SearchViewModel
{
    public string Query { get; set; } = string.Empty;

    public List<ArticleListDto> Results { get; set; } = [];
}

public class HomeIndexViewModel
{
    public List<ArticleListDto> RecentArticles { get; set; } = [];

    public List<OpenSourceProjectDto> OpenSourceProjects { get; set; } = [];
}

public class TimelineViewModel
{
    public List<TimelineGroupViewModel> Groups { get; set; } = [];

    public List<CategoryDto> Categories { get; set; } = [];

    public List<TagDto> Tags { get; set; } = [];

    public string? ActiveCategorySlug { get; set; }

    public string? ActiveTagSlug { get; set; }

    public string? ActiveCategoryName { get; set; }

    public string? ActiveTagName { get; set; }
}

public class TimelineGroupViewModel
{
    public string Label { get; set; } = null!;

    public List<ArticleListDto> Articles { get; set; } = [];
}

public class TagArticlesViewModel
{
    public TagDto Tag { get; set; } = null!;

    public List<ArticleListDto> Articles { get; set; } = [];
}

public class BlogDetailViewModel
{
    public ArticleDetailDto Article { get; set; } = null!;

    public List<CommentDto> Comments { get; set; } = [];

    public int CommentCount { get; set; }

    public PostCommentViewModel CommentForm { get; set; } = new();
}

public class PostCommentViewModel
{
    public long ArticleId { get; set; }

    public string Slug { get; set; } = null!;

    public long? ParentId { get; set; }

    public string? ReplyToNickName { get; set; }

    [Display(Name = "昵称")]
    [Required(ErrorMessage = "请填写昵称")]
    [StringLength(50)]
    public string NickName { get; set; } = string.Empty;

    [Display(Name = "邮箱")]
    [Required(ErrorMessage = "请填写邮箱")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "网站")]
    [StringLength(200)]
    public string? Website { get; set; }

    [Display(Name = "评论内容")]
    [Required(ErrorMessage = "请填写评论内容")]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;
}
