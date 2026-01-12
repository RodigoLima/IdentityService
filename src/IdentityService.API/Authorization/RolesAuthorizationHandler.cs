using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Extensions;

namespace IdentityService.API.Authorization;

public class RolesAuthorizationHandler : AuthorizationHandler<RolesRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(c => c.Type == ClaimTypes.Role)?.Value;

        if (!string.IsNullOrWhiteSpace(roleClaim) && Enum.TryParse<AccessLevel>(roleClaim, out var userRoles))
        {
            if (requirement.AccessLevel.HasAnyFlag(userRoles))
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}
