using Microsoft.AspNetCore.Mvc;
using IdentityService.API.Filters;
using IdentityService.Application.Dto;
using IdentityService.Application.Interfaces;

namespace IdentityService.API.Controllers;

[Route("accounts")]
public class AccountController(ILogger<AccountController> logger, ITokenApplicationService _tokenApplicationService) : BaseController(logger)
{
    ///<summary>
    ///Gera o token a partir de um usuário e senha
    ///</summary>
    /// <remarks>
    /// Obs: Obrigatório informar o email e senha do usuário
    /// </remarks>
    /// <param name="userLogin">Objeto com email e senha do usuário</param>
    /// <returns>Um token de autenticação</returns>
    [HttpPost("token")]
    [Produces("application/json")]
    [SkipUserFilter]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<object> GetToken([FromBody] UserLogin userLogin)
    {
        var token = await _tokenApplicationService.GetToken(userLogin);

        if (string.IsNullOrEmpty(token))
            return Unauthorized();

        return Ok(token);
    }
}
