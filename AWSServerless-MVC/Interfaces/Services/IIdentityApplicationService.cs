using AWSServerless_MVC.Dtos;
using AWSServerless_MVC.Entities;

namespace AWSServerless_MVC.Interfaces.Services
{
    public interface IIdentityApplicationService
    {
        Task<JwtToken> ExternalLoginCallbackAsync(string? remoteError = null, string? callback = null, string role = "");
        Task<JwtToken> RefreshTokenAsync(RefreshTokenInput input);
    }
}
