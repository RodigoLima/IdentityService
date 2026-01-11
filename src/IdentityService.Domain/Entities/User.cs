using IdentityService.Domain.Enums;

namespace IdentityService.Domain.Entities;

public class User : BaseEntity
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public AccessLevel? AccessLevel { get; set; }

    public User() : base() { }

    public void SetDefaultUser() => AccessLevel = Enums.AccessLevel.User;
    public void SetAdminUser() => AccessLevel = Enums.AccessLevel.Admin;
}
