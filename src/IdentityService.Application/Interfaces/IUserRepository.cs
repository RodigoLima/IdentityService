using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> ObterPorIdAsync(Guid id);
    Task<User?> ObterPorEmailAsync(string email);
    Task<User> CriarAsync(User user);
    Task<User> AtualizarAsync(User user);
}
