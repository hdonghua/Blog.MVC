using BeaverX.Core.Dependency;
using Microsoft.AspNetCore.Identity;

namespace Blog.MVC.Services.Users;

public class PasswordHasher : IPasswordHasher, ISingletonDependency
{
    private readonly PasswordHasher<PasswordUser> _hasher = new();

    public string HashPassword(string password) =>
        _hasher.HashPassword(new PasswordUser(), password);

    public bool VerifyPassword(string hashedPassword, string password) =>
        _hasher.VerifyHashedPassword(new PasswordUser(), hashedPassword, password)
            != PasswordVerificationResult.Failed;

    private sealed class PasswordUser;
}
