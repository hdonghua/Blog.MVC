using BeaverX.Core.Dependency;
using BeaverX.Domain.Repositories;
using Blog.MVC.Data;
using Blog.MVC.IServices.Users;
using Blog.MVC.IServices.Users.Dtos;
using Blog.MVC.Models.Common;
using Blog.MVC.Models.Users;
using Blog.MVC.Services.Users;
using Microsoft.EntityFrameworkCore;

namespace Blog.MVC.Services.Users;

public class UserAppService : IUserAppService, IScopedDependency
{
    private readonly IRepository<AppUser> _userRepository;
    private readonly BlogDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UserAppService(
        IRepository<AppUser> userRepository,
        BlogDbContext dbContext,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<PagedResult<UserDto>> GetPagedListAsync(int page = 1, int pageSize = PaginationHelper.DefaultPageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);

        var query = _dbContext.Users.AsNoTracking().OrderBy(x => x.Id);
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>
        {
            Items = users.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindAsync(id, cancellationToken);
        return user == null ? null : MapToDto(user);
    }

    public Task<AppUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default) =>
        _userRepository.FindAsync(x => x.UserName == userName, cancellationToken: cancellationToken);

    public async Task UpdateAsync(UpdateUserDto input, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindAsync(input.Id, cancellationToken)
            ?? throw new InvalidOperationException("用户不存在");

        var duplicateUserName = await _userRepository.FindAsync(
            x => x.UserName == input.UserName && x.Id != input.Id,
            cancellationToken: cancellationToken);
        if (duplicateUserName != null)
        {
            throw new InvalidOperationException("用户名已存在");
        }

        var duplicateEmail = await _userRepository.FindAsync(
            x => x.Email == input.Email && x.Id != input.Id,
            cancellationToken: cancellationToken);
        if (duplicateEmail != null)
        {
            throw new InvalidOperationException("邮箱已存在");
        }

        user.UserName = input.UserName.Trim();
        user.Email = input.Email.Trim();
        user.DisplayName = input.DisplayName?.Trim();

        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(input.Password);
        }

        await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
    }

    public bool VerifyPassword(AppUser user, string password) =>
        _passwordHasher.VerifyPassword(user.PasswordHash, password);

    private static UserDto MapToDto(AppUser user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        DisplayName = user.DisplayName,
        CreationTime = user.CreationTime
    };
}
