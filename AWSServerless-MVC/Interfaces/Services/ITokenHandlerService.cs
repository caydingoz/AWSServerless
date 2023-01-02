using AWSServerless_MVC.Entities;
using AWSServerless_MVC.Models;
using System.Security.Claims;

namespace AWSServerless_MVC.Interfaces.Services
{
    public interface ITokenHandlerService
    {
        public Task<JwtToken> CreateTokenAsync(ApplicationUser user, IList<string>? userRoles);
        public IEnumerable<Claim> GetPrincipalFromExpiredToken(string token);
    }
}
