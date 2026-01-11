using System.Text.Json.Serialization;

namespace IdentityService.Application.Dto;

public class Identifier
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }

    public Identifier() { }
}
