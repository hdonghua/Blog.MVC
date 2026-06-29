using Blog.MVC.IServices.Users.Dtos;
using Blog.MVC.Models.Users;

namespace Blog.MVC.IServices.Users;

public interface IUserAppService
{
    Task<List<UserDto>> GetListAsync(CancellationToken cancellationToken = default);

    Task<UserDto?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task<AppUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateUserDto input, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateUserDto input, CancellationToken cancellationToken = default);

    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    bool VerifyPassword(AppUser user, string password);
}
