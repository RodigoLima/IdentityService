using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityService.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SkipUserFilterAttribute : Attribute, IFilterMetadata { }
