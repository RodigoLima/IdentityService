namespace IdentityService.Domain.Interfaces.Security;

public interface IPasswordHasher
{
    /// <summary>
    /// Gera um hash da senha fornecida
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <returns>Hash da senha</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifica se a senha fornecida corresponde ao hash armazenado
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <param name="hashedPassword">Hash armazenado</param>
    /// <returns>True se a senha corresponde ao hash, False caso contrário</returns>
    bool VerifyPassword(string password, string hashedPassword);
}
