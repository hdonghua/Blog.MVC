using BeaverX.Domain.Entities;

namespace Blog.MVC.Models.Users;

public class AppUser : FullAuditedEntity
{
    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Avatar { get; set; }

    public UserRole Role { get; set; } = UserRole.Author;

    public bool IsActive { get; set; } = true;
}

public enum UserRole
{
    Admin = 1,
    Author = 2,
    Reader = 3
}
