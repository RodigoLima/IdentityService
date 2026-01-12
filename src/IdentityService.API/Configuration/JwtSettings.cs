namespace IdentityService.Api.Configuration;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "IdentityService";
}
