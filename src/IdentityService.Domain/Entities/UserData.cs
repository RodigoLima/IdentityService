using System.Security.Claims;

namespace IdentityService.Domain.Entities;

public class UserData : BaseEntity
{
    public string? Email { get; set; }
    public string? Name { get; set; }

    public void Set(UserData userData)
    {
        Id = userData.Id;
        Email = userData.Email;
        Name = userData.Name;
    }

    public void Set(ClaimsPrincipal user)
    {
        var id = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        Id = string.IsNullOrEmpty(id) ? Guid.Empty : Guid.Parse(id);
        Email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
        Name = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty;
    }

    public void Set(User user)
    {
        Id = user.Id;
        Email = user.Email ?? string.Empty;
        Name = user.Name ?? string.Empty;
    }
}
