using System.ComponentModel.DataAnnotations;

namespace Blog.MVC.ViewModels.Admin;

public class LoginViewModel
{
    [Required(ErrorMessage = "请输入用户名")]
    [Display(Name = "用户名")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "请输入密码")]
    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    public string Password { get; set; } = null!;

    [Display(Name = "记住我")]
    public bool RememberMe { get; set; }
}

public class UserFormViewModel
{
    public long? Id { get; set; }

    [Required(ErrorMessage = "请输入用户名")]
    [StringLength(50)]
    [Display(Name = "用户名")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "请输入邮箱")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    [StringLength(100)]
    [Display(Name = "邮箱")]
    public string Email { get; set; } = null!;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码至少 6 位")]
    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    public string? Password { get; set; }

    [StringLength(50)]
    [Display(Name = "显示名称")]
    public string? DisplayName { get; set; }
}
