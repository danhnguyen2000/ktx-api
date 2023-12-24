using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KTX_BLL.Service
{
    public class UserService
    {
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Login(int expiredDate = 1)
        {
            try
            {
                string validIssusers = _configuration.GetValue<string>("ApiUrl");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = validIssusers,              // Not required as no third-party is involved
                    Audience = null,            // Not required as no third-party is involved
                    IssuedAt = DateTime.Now,
                    NotBefore = DateTime.Now,
                    Expires = DateTime.Now.AddDays(expiredDate),
                    Subject = new ClaimsIdentity(new List<Claim> {
                    }),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Convert.FromBase64String("TXlOYW1lSXNEYW5oVGhpc0lzTXlTZWNyZXRLZXk=")), 
                    SecurityAlgorithms.HmacSha256Signature)
                };
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
                var token = jwtTokenHandler.WriteToken(jwtToken);
                Console.Write(token);
                return token;

            }
            catch (Exception e)
            {
                if (e.Source != null)
                    Console.WriteLine("Exception source: {0}", e.Message);
            }
            return "Error generate token";
        }
    }
}
