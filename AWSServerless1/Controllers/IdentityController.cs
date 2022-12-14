//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Patika.Framework.Shared.Entities;

//namespace AWSServerless1.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class IdentityController : ControllerBase
//    {
//        UserManager<ApplicationUser> UserManager { get; }
//        IUserStore<ApplicationUser> UserStore { get; }
//        SignInManager<ApplicationUser> SignInManager { get; }

//        public IdentityController(IServiceProvider serviceProvider)
//        {
//            UserManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
//            UserStore = serviceProvider.GetService<IUserStore<ApplicationUser>>();
//            SignInManager = serviceProvider.GetService<SignInManager<ApplicationUser>>();
//        }
//        [HttpGet("deneme")]
//        public IActionResult Deneme()
//        {
//            return Ok("okk");
//        }

//        [HttpGet("okta")]
//        public IActionResult LoginWithOktaAsync()
//        {
//            //var properties = new AuthenticationProperties();
//            //properties.Items.Add("sessionToken", sessionToken);
//            //properties.RedirectUri = "/Home/";

//            //return Challenge(properties, OktaDefaults.MvcAuthenticationScheme);
//            var callback = "identity/deneme";
//            var provider = OpenIdConnectDefaults.AuthenticationScheme;
//            var url = $"/identity/authorize/callback?callback={callback}";

//            if (string.IsNullOrEmpty(callback))
//            {
//                throw new Exception("Callback is empty");
//            }
//            var properties = SignInManager.ConfigureExternalAuthenticationProperties(provider, url);
//            return new ChallengeResult(provider, properties);
//        }

//        [HttpGet("authorize/callback")]
//        public async Task<IActionResult> RegisterCallbackAsync(string callback)
//        {
//            var info = await SignInManager.GetExternalLoginInfoAsync();
//            var result = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

//            if (result.Succeeded)
//                return Ok("ok");
//            else
//                return BadRequest("bad");
//        }
//    }
//}
