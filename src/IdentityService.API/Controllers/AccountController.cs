using IdentityService.Api.DTOs;
using IdentityService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[Route("api/auth")]
public class AccountController : ControllerBase
{
    private readonly TokenService _tokenService;

    public AccountController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var token = await _tokenService.GerarTokenAsync(dto.Email, dto.Senha);

        if (string.IsNullOrEmpty(token))
            return Unauthorized();

        return Ok(new { token });
    }
}
