using IdentityService.Application.Dto;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Interfaces;
using IdentityService.Domain.Interfaces.Security;

namespace IdentityService.Application.Services;

public class TokenApplicationService(IUserService userService, ITokenService tokenService, IPasswordHasher passwordHasher) : ITokenApplicationService
{
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async Task<string> GetToken(UserLogin userLogin)
    {
        var user = await userService.GetByEmail(userLogin.Email);

        if (string.IsNullOrWhiteSpace(user.Password))
            throw new UnauthorizedAccessException("Senha inválida");

        // Verifica se a senha fornecida corresponde ao hash armazenado
        bool isPasswordValid = _passwordHasher.VerifyPassword(userLogin.Password ?? string.Empty, user.Password);

        // Se o hash não funcionou, tenta comparar em texto plano (para senhas antigas)
        // Se funcionar, atualiza automaticamente para hash
        if (!isPasswordValid && user.Password == userLogin.Password)
        {
            // Senha antiga em texto plano detectada - atualizar para hash
            await userService.UpdatePassword(user, userLogin.Password ?? string.Empty);
            isPasswordValid = true;
        }

        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Senha inválida");

        return tokenService.GenerateToken(user);
    }

    public async Task<string> GetTokenByAutorization(string? email)
    {
        var user = await userService.GetByEmail(email);

        return tokenService.GenerateToken(user, force: true);
    }
}
