using AWSServerless_MVC.Consts;
using AWSServerless_MVC.Dtos;
using AWSServerless_MVC.Entities;
using AWSServerless_MVC.Interfaces.Services;
using AWSServerless_MVC.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AWSServerless_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        SignInManager<ApplicationUser> SignInManager { get; }
        IIdentityApplicationService IdentityApplicationService { get; }

        public IdentityController(IServiceProvider serviceProvider)
        {
            IdentityApplicationService = serviceProvider.GetService<IIdentityApplicationService>() ?? throw new ArgumentNullException();
            SignInManager = serviceProvider.GetService<SignInManager<ApplicationUser>>() ?? throw new ArgumentNullException();
        }

        [HttpGet("login/okta")]
        public IActionResult LoginWithOktaAsync([FromQuery]string callback)
        {
            var provider = OpenIdConnectDefaults.AuthenticationScheme;
            var url = $"api/identity/callback?callback={callback}&role={RoleConsts.USER_ROLE}";

            if (string.IsNullOrEmpty(callback))
                throw new Exception("Callback is empty");
            
            var properties = SignInManager.ConfigureExternalAuthenticationProperties(provider, url);
            
            return new ChallengeResult(provider, properties);
        }

        [HttpGet("callback")]
        public async Task<JwtToken> ExternalLoginCallbackAsync(string? remoteError = null, string? callback = null, string role = "")
        {
            return await IdentityApplicationService.ExternalLoginCallbackAsync(remoteError, callback, role);
        }
        [HttpPost("refresh-token")]
        public async Task<JwtToken> RefreshTokenAsync(RefreshTokenInput input)
        {
            return await IdentityApplicationService.RefreshTokenAsync(input);
        }
    }
}
