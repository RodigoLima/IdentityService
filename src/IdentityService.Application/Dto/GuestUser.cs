using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IdentityService.Application.Dto;

public class GuestUser
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "O campo nome é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    [RegularExpression(@"^[a-zA-ZÀ-ÿ\s'-]+$", ErrorMessage = "O nome pode conter apenas letras e espaços.")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    [Required(ErrorMessage = "O campo email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    public string? Email { get; set; }

    [JsonPropertyName("password")]
    [Required(ErrorMessage = "A senha é obrigatória.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).{8,}$",
        ErrorMessage = "A senha deve ter pelo menos 8 caracteres, incluindo uma letra maiúscula, um número e um caractere especial.")]
    public string? Password { get; set; }

    public GuestUser() : base() { }
}
