using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Application.Dto;

namespace IdentityService.Application.Interfaces;

public interface ITokenApplicationService
{
    Task<string> GetToken(UserLogin userLogin);
    Task<string> GetTokenByAutorization(string? email);
}
