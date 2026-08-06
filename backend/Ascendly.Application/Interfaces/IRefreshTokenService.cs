    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Ascendly.Domain.Entities;

    namespace Ascendly.Application.Interfaces
    {
        public interface IRefreshTokenService
        {
            RefreshToken Generate(User user);
        }
    }
