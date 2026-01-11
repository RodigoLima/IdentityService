using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces.Security;

public interface ITokenService
{
    string GenerateToken(User user, bool force = false);
}
