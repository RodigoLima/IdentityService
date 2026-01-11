using Microsoft.AspNetCore.Authorization;
using IdentityService.Domain.Enums;

namespace IdentityService.API.Authorization;

  public class RolesRequirement(AccessLevel level) : IAuthorizationRequirement
  {
      public AccessLevel AccessLevel { get; } = level;
  }
