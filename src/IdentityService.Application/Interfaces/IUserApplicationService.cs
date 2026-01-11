using IdentityService.Application.Dto;

namespace IdentityService.Application.Interfaces;

public interface IUserApplicationService
{
    Task<User> Add(GuestUser model);
    Task<User> AddAdmin(GuestUser model);
}
