using IdentityService.Api.DTOs;
using IdentityService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[Route("api/auth")]
public class AccountController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(TokenService tokenService, ILogger<AccountController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Tentativa de login para o email: {Email}", dto.Email);
        
        var token = await _tokenService.GerarTokenAsync(dto.Email, dto.Senha);

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Falha no login para o email: {Email}", dto.Email);
            return Unauthorized();
        }

        _logger.LogInformation("Login realizado com sucesso para o email: {Email}", dto.Email);
        return Ok(new { token });
    }
}
