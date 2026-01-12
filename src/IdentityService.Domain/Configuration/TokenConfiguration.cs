namespace IdentityService.Domain.Configuration;

public class TokenConfiguration
{
    public required string Key { get; set; } 
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationTimeHour { get; set; }
    public int IncreaseExpirationTimeMinutes { get; set; }
}
