using System.Text.Json.Serialization;
using IdentityService.Domain.Enums;

namespace IdentityService.Application.Dto;

public class User : BaseModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("access_level")]
    public AccessLevel? AccessLevel { get; set; }

    public User() : base() { }
}
