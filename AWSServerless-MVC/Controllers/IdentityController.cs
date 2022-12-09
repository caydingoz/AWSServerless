using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Okta.AspNetCore;
using Patika.Framework.Shared.Entities;

namespace AWSServerless_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        UserManager<ApplicationUser> UserManager { get; }
        IUserStore<ApplicationUser> UserStore { get; }
        SignInManager<ApplicationUser> SignInManager { get; }

        public IdentityController(IServiceProvider serviceProvider)
        {
            UserManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            UserStore = serviceProvider.GetService<IUserStore<ApplicationUser>>();
            SignInManager = serviceProvider.GetService<SignInManager<ApplicationUser>>();
        }
        [HttpGet("deneme")]
        public IActionResult Deneme()
        {
            return Ok("okk");
        }

        [HttpGet("okta")]
        public IActionResult LoginWithOktaAsync()
        {
            var callback = "https://localhost:5001/api/identity/deneme";
            var provider = OpenIdConnectDefaults.AuthenticationScheme;
            var url = $"api/identity/authorize/callback?callback={callback}";

            if (string.IsNullOrEmpty(callback))
            {
                throw new Exception("Callback is empty");
            }
            var properties = SignInManager.ConfigureExternalAuthenticationProperties(provider, url);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet("~/authorization-code/api/identity/authorize/callback")]
        public async Task<IActionResult> RegisterCallbackAsync(string? remoteError = null, string? callback = null)
        {
            var info = await SignInManager.GetExternalLoginInfoAsync();
            var result = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            
            if (result.Succeeded)
                return Ok("ok");
            else
                return BadRequest("bad");
        }
    }
}
