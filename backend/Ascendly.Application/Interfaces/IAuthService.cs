using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Auth;

namespace Ascendly.Application.Interfaces
{
    public interface IAuthService
    {
       public Task<bool> RegisterAsync(RegisterRequest request);

        public Task<string?> LoginAsync(LoginRequest request);
    }
}
