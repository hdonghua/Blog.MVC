namespace Blog.MVC.IServices.Users.Dtos;

public class UserDto
{
    public long Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }
}

public class CreateUserDto
{
    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}

public class UpdateUserDto
{
    public long Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? DisplayName { get; set; }

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
