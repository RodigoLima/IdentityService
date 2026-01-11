using IdentityService.Domain.Interfaces.Security;

namespace IdentityService.Domain.Services.Security;

public class PasswordHasherService : IPasswordHasher
{
    /// <summary>
    /// Gera um hash da senha usando BCrypt com work factor padrão (10)
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("A senha não pode ser vazia.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(10));
    }

    /// <summary>
    /// Verifica se a senha fornecida corresponde ao hash armazenado
    /// </summary>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            // Se o hash não for um formato BCrypt válido, retorna false
            // Isso permite que senhas antigas em texto plano sejam migradas gradualmente
            return false;
        }
    }
}
