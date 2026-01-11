
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Domain.Interfaces.Infraestructure;

namespace IdentityService.Domain.Services;

public class UserService(IUserRepository userRepository, UserData userData) : BaseService<User>(userRepository, userData), IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<User> GetById(Guid id)
    {
        return await _userRepository.GetById(id, false, false);
    }

    public async Task<User> GetByEmail(string? email)
    {
        return await _userRepository.GetByEmail(email);
    }

    public override async Task<User> Add(User entity)
    {
        await ValidateUserDoesNotExist(entity.Email);
        entity.SetDefaultUser();
        entity.PrepareToInsert(_userData.Id);
        return await base.Add(entity);
    }

    public async Task<User> AddAdmin(User entity)
    {
        await ValidateUserDoesNotExist(entity.Email);
        entity.SetAdminUser();
        entity.PrepareToInsert(_userData.Id);
        return await base.Add(entity);
    }

    private async Task ValidateUserDoesNotExist(string? email)
    {
        var existingUser = await _userRepository.GetByEmail(email);
        if (existingUser != null)
            throw new ArgumentException("O usuário já existe.");
    }
}
