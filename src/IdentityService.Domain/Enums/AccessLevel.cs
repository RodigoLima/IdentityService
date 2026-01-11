namespace IdentityService.Domain.Enums;

[Flags]
public enum AccessLevel
{
    None = 0,
    Admin = 1,
    User = 2,
    Guest = 4
}
