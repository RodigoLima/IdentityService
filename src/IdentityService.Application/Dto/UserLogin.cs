using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IdentityService.Application.Dto;

public class UserLogin
{
    [JsonPropertyName("email")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    public string? Email { get; set; }

    [JsonPropertyName("password")]
    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string? Password { get; set; }
}
