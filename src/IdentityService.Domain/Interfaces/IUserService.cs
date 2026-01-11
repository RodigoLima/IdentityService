using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

public interface IUserService : IBaseService<User>
{
    Task<User> GetById(Guid id);
    Task<User> GetByEmail(string? email);
    Task<User> AddAdmin(User entity);
}
