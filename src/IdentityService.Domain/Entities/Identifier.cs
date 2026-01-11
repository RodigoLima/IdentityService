using IdentityService.Domain.Entities.Interfaces;

namespace IdentityService.Domain.Entities;

public class Identifier : IIdentifier
{
    public Guid Id { get; set; }
}
