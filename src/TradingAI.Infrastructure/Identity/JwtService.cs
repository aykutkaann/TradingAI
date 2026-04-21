using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Identity
{
    public class JwtService :IJwtService
    {
        public  string GenerateAccessToken(User user)
        {
            return "generated-access-token";
        }
        public  RefreshToken GetRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress
                
            };

        }
    }
}
