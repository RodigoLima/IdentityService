using AutoMapper;
using IdentityService.Application.Dto;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Interfaces;
using EN = IdentityService.Domain.Entities;

namespace IdentityService.Application.Services;

public class UserApplicationService(IUserService userService, IMapper mapper) : IUserApplicationService
{
    public async Task<User> Add(GuestUser model)
    {
        var user = mapper.Map<EN.User>(model);
        user = await userService.Add(user);
        return mapper.Map<User>(user);
    }

    public async Task<User> AddAdmin(GuestUser model)
    {
        var user = mapper.Map<EN.User>(model);
        user = await userService.AddAdmin(user);
        return mapper.Map<User>(user);
    }
}
