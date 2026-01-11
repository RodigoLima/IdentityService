using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityService.API.Constants;
using IdentityService.API.Filters;
using IdentityService.Application.Dto;
using IdentityService.Application.Interfaces;

namespace IdentityService.API.Controllers;

[Route("users")]
[ApiController]
public class UserController(ILogger<UserController> logger, IUserApplicationService userApplicationService) : BaseController(logger)
{
    private readonly IUserApplicationService _userApplicationService = userApplicationService;

    /// <summary>
    /// Criar um novo usuário
    /// </summary>
    /// <param name="user">Objeto com as propriedades para criar um novo usuário</param>
    /// <returns>Um objeto do usuário criado</returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [Authorize(Policy = AppConstants.Policies.Admin)]
    public async Task<object> Create([FromBody] GuestUser user)
    {
        var entity = await _userApplicationService.Add(user);
        return Ok(entity);
    }

    /// <summary>
    /// Criar um novo usuário
    /// </summary>
    /// <param name="user">Objeto com as propriedades para criar um novo usuário</param>
    /// <returns>Um objeto do usuário criado</returns>
    [HttpPost("admin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [SkipUserFilter]
    public async Task<object> CreateAdmin([FromBody] GuestUser user)
    {
        var entity = await _userApplicationService.AddAdmin(user);
        return Ok(entity);
    }
}
