using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces.Security;

namespace IdentityService.Application.Services;

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository repository, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> ObterPorIdAsync(Guid id)
        => await _repository.ObterPorIdAsync(id);

    public async Task<User?> ObterPorEmailAsync(string email)
        => await _repository.ObterPorEmailAsync(email);

    public async Task<User> CriarAsync(string nome, string email, string senha, bool isAdmin = false)
    {
        // Verifica se o usuário já existe
        var existingUser = await _repository.ObterPorEmailAsync(email);
        if (existingUser != null)
            throw new ArgumentException("O usuário já existe.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = nome,
            Email = email,
            Password = _passwordHasher.HashPassword(senha),
            AccessLevel = isAdmin ? Domain.Enums.AccessLevel.Admin : Domain.Enums.AccessLevel.User,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.CriarAsync(user);
    }

    public async Task<bool> VerificarSenhaAsync(string email, string senha)
    {
        var user = await _repository.ObterPorEmailAsync(email);
        if (user == null || string.IsNullOrWhiteSpace(user.Password))
            return false;

        // Verifica se a senha fornecida corresponde ao hash armazenado
        bool isPasswordValid = _passwordHasher.VerifyPassword(senha, user.Password);

        // Se o hash não funcionou, tenta comparar em texto plano (para senhas antigas)
        // Se funcionar, atualiza automaticamente para hash
        if (!isPasswordValid && user.Password == senha)
        {
            // Senha antiga em texto plano detectada - atualizar para hash
            user.Password = _passwordHasher.HashPassword(senha);
            await _repository.AtualizarAsync(user);
            isPasswordValid = true;
        }

        return isPasswordValid;
    }
}
