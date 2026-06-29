namespace Blog.MVC.IServices.Users.Dtos;

public class UserDto
{
    public long Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? DisplayName { get; set; }

    public DateTime CreationTime { get; set; }
}

public class UpdateUserDto
{
    public long Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? DisplayName { get; set; }
}
