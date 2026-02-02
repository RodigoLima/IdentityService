using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Domain.Interfaces.Infraestructure;
using IdentityService.Domain.Interfaces.Security;

namespace IdentityService.Domain.Services;

public class UserService(IUserRepository userRepository, UserData userData, IPasswordHasher passwordHasher) : BaseService<User>(userRepository, userData), IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

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
        HashPasswordIfProvided(entity);
        entity.SetDefaultUser();
        entity.PrepareToInsert(_userData.Id);
        return await base.Add(entity);
    }

    public async Task<User> AddAdmin(User entity)
    {
        await ValidateUserDoesNotExist(entity.Email);
        HashPasswordIfProvided(entity);
        entity.SetAdminUser();
        entity.PrepareToInsert(_userData.Id);
        return await base.Add(entity);
    }

    public async Task UpdatePassword(User user, string newPassword)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("A senha não pode ser vazia.", nameof(newPassword));

        user.Password = _passwordHasher.HashPassword(newPassword);
        user.PrepareToUpdate(_userData.Id);
        await base.Update(user);
    }

    private void HashPasswordIfProvided(User entity)
    {
        if (!string.IsNullOrWhiteSpace(entity.Password))
        {
            entity.Password = _passwordHasher.HashPassword(entity.Password);
        }
    }

    private async Task ValidateUserDoesNotExist(string? email)
    {
        try
        {
            await _userRepository.GetByEmail(email);
            throw new ArgumentException("O usuário já existe.");
        }
        catch (InvalidOperationException)
        {
        }
    }
}
