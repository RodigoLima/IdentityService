using IdentityService.Api.DTOs;
using IdentityService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioDto dto)
    {
        try
        {
            var user = await _userService.CriarAsync(dto.Nome, dto.Email, dto.Senha, isAdmin: false);
            return CreatedAtAction(nameof(ObterPorId), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("admin")]
    public async Task<IActionResult> CriarAdmin([FromBody] CriarUsuarioDto dto)
    {
        try
        {
            var user = await _userService.CriarAsync(dto.Nome, dto.Email, dto.Senha, isAdmin: true);
            return CreatedAtAction(nameof(ObterPorId), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var user = await _userService.ObterPorIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }
}
