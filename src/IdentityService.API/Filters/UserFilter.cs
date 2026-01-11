using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Configuration;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Helpers;

namespace IdentityService.API.Filters;

public class UserFilter(UserData userData, ITokenApplicationService tokenApplicationService, IOptions<TokenConfiguration> options) : IAuthorizationFilter
{
    private readonly UserData _userData = userData;
    private readonly ITokenApplicationService _tokenApplicationService = tokenApplicationService;
    private readonly TokenConfiguration _configuration = options.Value;

    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.Filters.Any(f => f is SkipUserFilterAttribute))
            return;

        var user = context.HttpContext.User;
        var userId = user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var token = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (token != null)
        {
            _userData.Set(TokenHelper.GetUserData(token, _configuration.Key));
            var timeUntilExpiration = TokenHelper.GetTimeUntilExpiration(token, _configuration.Key);

            if (timeUntilExpiration.HasValue && timeUntilExpiration.Value.TotalMinutes <= 5)
            {
                token = await _tokenApplicationService.GetTokenByAutorization(_userData.Email);
                context.HttpContext.Response.Cookies.Append("AuthToken", token);
            }

            if (context.HttpContext.User?.Claims?.Count() <= 0)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
        else
        {
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}
