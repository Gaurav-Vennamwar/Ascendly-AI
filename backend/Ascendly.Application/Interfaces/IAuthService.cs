using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Auth;

namespace Ascendly.Application.Interfaces
{
    internal interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequest request);

        Task<string?> LoginAsync(LoginRequest request);
    }
}
