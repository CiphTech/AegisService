using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Aegis.Service;
using Microsoft.AspNetCore.Mvc;

namespace Aegis.Rest.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly Dictionary<string, byte[]> _testLogins;
        
        [HttpPost("login")]
        public async Task<IActionResult> GetTokenAsync([FromBody] string user, [FromBody] string password)
        {
            IIdentity identity = GetUserIdentity(user, PrimitiveHelper.ComputeHash(password));

            if (identity is null)
                return BadRequest($"Authentication for user '{user}' failed");

            DateTime now = DateTime.Now;

            throw new NotImplementedException();
        }

        public AuthController()
        {
            _testLogins = new Dictionary<string, byte[]>
            {
                ["yoba1"] = PrimitiveHelper.StringToByteArray("022bdf3d14dd7a21d648716c64b1c8d2ca31f8e57bf27ffd9f7abf4fcd2882c5"),
                ["yoba2"] = PrimitiveHelper.StringToByteArray("44d803ae119e67ce6d598c258769071b3fe97c304a0e277325ba12350f4134ca")
            };
        }

        private IIdentity GetUserIdentity(string name, byte[] hash)
        {
            if (!_testLogins.TryGetValue(name, out byte[] storedHash))
                return null;

            if (PrimitiveHelper.ArrayEquals(hash, storedHash))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, name)
                };
                
                return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            }

            return null;
        }
    }
}