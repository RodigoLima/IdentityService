using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Extensions;

namespace IdentityService.API.Authorization;

public class RolesAuthorizationHandler : AuthorizationHandler<RolesRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(c => c.Type == ClaimTypes.Role)?.Value;

        if (roleClaim.IsNullOrEmpty() == false && Enum.TryParse<AccessLevel>(roleClaim, out var userRoles))
        {
            if (requirement.AccessLevel.HasAnyFlag(userRoles))
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}
