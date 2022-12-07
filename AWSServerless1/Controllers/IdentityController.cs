using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Patika.Framework.Shared.Entities;

namespace AWSServerless1.Controllers
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
    }
}
