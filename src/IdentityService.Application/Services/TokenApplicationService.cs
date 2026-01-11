using IdentityService.Application.Dto;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Interfaces;
using IdentityService.Domain.Interfaces.Security;

namespace IdentityService.Application.Services;

public class TokenApplicationService(IUserService userService, ITokenService tokenService) : ITokenApplicationService
{
    public async Task<string> GetToken(UserLogin userLogin)
    {
        var user = await userService.GetByEmail(userLogin.Email)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado");

        if (user.Password != userLogin.Password)
            throw new UnauthorizedAccessException("Senha inválida");

        return tokenService.GenerateToken(user);
    }

    public async Task<string> GetTokenByAutorization(string? email)
    {
        var user = await userService.GetByEmail(email)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado");

        return tokenService.GenerateToken(user, force: true);
    }
}
