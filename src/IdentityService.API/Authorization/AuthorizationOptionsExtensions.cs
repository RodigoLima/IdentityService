using Microsoft.AspNetCore.Authorization;
using IdentityService.Domain.Enums;

namespace IdentityService.API.Authorization;

public static class AuthorizationOptionsExtensions
{
    public static AuthorizationOptions AddPolicyWithPermission(this AuthorizationOptions options, string policyName, AccessLevel accessLevel)
    {
        options.AddPolicy(policyName, policy => policy.Requirements.Add(new RolesRequirement(accessLevel)));
        return options;
    }
}
