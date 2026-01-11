namespace IdentityService.Api.Configuration;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "IdentityService";
    public string Audience { get; set; } = "IdentityService";
}
