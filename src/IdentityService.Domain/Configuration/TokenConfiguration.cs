using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Domain.Configuration;

public class TokenConfiguration
{
    public required string Key { get; set; } 
    public int ExpirationTimeHour { get; set; }
    public int IncreaseExpirationTimeMinutes { get; set; }
}
