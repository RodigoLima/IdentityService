using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces.Security;
using IdentityService.Domain.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityService.Application.Services;

public class TokenService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TokenConfiguration _configuration;

    public TokenService(
        IUserRepository userRepository, 
        IPasswordHasher passwordHasher,
        IOptions<TokenConfiguration> configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration.Value;
    }

    public async Task<string?> GerarTokenAsync(string email, string senha)
    {
        var user = await _userRepository.ObterPorEmailAsync(email);
        if (user == null)
            return null;

        if (string.IsNullOrWhiteSpace(user.Password))
            return null;

        // Verifica se a senha fornecida corresponde ao hash armazenado
        bool isPasswordValid = _passwordHasher.VerifyPassword(senha, user.Password);

        // Se o hash não funcionou, tenta comparar em texto plano (para senhas antigas)
        if (!isPasswordValid && user.Password == senha)
        {
            // Senha antiga em texto plano detectada - atualizar para hash
            user.Password = _passwordHasher.HashPassword(senha);
            await _userRepository.AtualizarAsync(user);
            isPasswordValid = true;
        }

        if (!isPasswordValid)
            return null;

        return CriarToken(user);
    }

    private string CriarToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_configuration.Key))
            throw new InvalidOperationException("JWT Key não configurada.");
        
        if (string.IsNullOrWhiteSpace(_configuration.Issuer))
            throw new InvalidOperationException("JWT Issuer não configurado.");

        var key = Convert.FromBase64String(_configuration.Key);
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("sub", user.Id.ToString()),
            new("userId", user.Id.ToString()),
            new(ClaimTypes.Name, user.Name ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        if (user.AccessLevel.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.Role, ((int)user.AccessLevel.Value).ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _configuration.Issuer,
            Expires = DateTime.UtcNow.AddHours(_configuration.ExpirationTimeHour),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
