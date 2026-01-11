using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces.Infraestructure;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetById(Guid id, bool include = false, bool tracking = false);
    Task<User> GetByEmail(string? email, bool include = false, bool tracking = false);
}
