namespace Blog.MVC.Services.Users;

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(string hashedPassword, string password);
}
