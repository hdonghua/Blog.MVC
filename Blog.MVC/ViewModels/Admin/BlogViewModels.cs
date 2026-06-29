using System.ComponentModel.DataAnnotations;
using Blog.MVC.IServices.Blog.Dtos;
using Blog.MVC.Models.Blog;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Blog.MVC.ViewModels.Admin;

public class CategoryFormViewModel
{
    public long? Id { get; set; }

    [Required(ErrorMessage = "请输入分类名称")]
    [StringLength(50)]
    [Display(Name = "名称")]
    public string Name { get; set; } = null!;

    [StringLength(80)]
    [Display(Name = "别名")]
    public string? Slug { get; set; }

    [StringLength(200)]
    [Display(Name = "描述")]
    public string? Description { get; set; }

    [Display(Name = "排序")]
    public int SortOrder { get; set; }
}

public class TagFormViewModel
{
    public long? Id { get; set; }

    [Required(ErrorMessage = "请输入标签名称")]
    [StringLength(30)]
    [Display(Name = "名称")]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    [Display(Name = "别名")]
    public string? Slug { get; set; }
}

public class ArticleFormViewModel
{
    public long? Id { get; set; }

    [Required(ErrorMessage = "请输入标题")]
    [StringLength(200)]
    [Display(Name = "标题")]
    public string Title { get; set; } = null!;

    [StringLength(220)]
    [Display(Name = "别名")]
    public string? Slug { get; set; }

    [StringLength(500)]
    [Display(Name = "摘要")]
    public string? Summary { get; set; }

    [Required(ErrorMessage = "请输入正文")]
    [Display(Name = "正文")]
    public string Content { get; set; } = null!;

    [StringLength(500)]
    [Display(Name = "封面图 URL")]
    public string? CoverImage { get; set; }

    [Required(ErrorMessage = "请选择分类")]
    [Display(Name = "分类")]
    public long CategoryId { get; set; }

    [Display(Name = "标签")]
    public List<long> SelectedTagIds { get; set; } = [];

    [Required(ErrorMessage = "请选择状态")]
    [Display(Name = "状态")]
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    public List<SelectListItem> Categories { get; set; } = [];

    public List<TagDto> Tags { get; set; } = [];
}

public class DashboardViewModel
{
    public int UserCount { get; set; }

    public int ArticleCount { get; set; }

    public int CategoryCount { get; set; }

    public int TagCount { get; set; }

    public string CurrentUserName { get; set; } = null!;
}
