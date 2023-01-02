using AWSServerless_MVC.Interfaces.Services;
using AWSServerless_MVC.Entities;
using AWSServerless_MVC.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using AWSServerless_MVC.Dtos;
using AWSServerless_MVC.Interfaces.Repositories;

namespace AWSServerless_MVC.Services
{
    public class IdentityApplicationService : IIdentityApplicationService
    {
        UserManager<ApplicationUser> UserManager { get; }
        IUserStore<ApplicationUser> UserStore { get; }
        SignInManager<ApplicationUser> SignInManager { get; }
        IUserEmailStore<ApplicationUser> EmailStore { get; set; }
        AuthConfiguration AuthConfiguration { get; set; }
        ITokenHandlerService TokenHandlerService { get; set; }
        IUserRefreshTokenRepository UserRefreshTokenRepository { get; set; }

        public IdentityApplicationService(IServiceProvider serviceProvider)
        {
            UserManager = serviceProvider.GetService<UserManager<ApplicationUser>>() ?? throw new ArgumentNullException();
            UserStore = serviceProvider.GetService<IUserStore<ApplicationUser>>() ?? throw new ArgumentNullException();
            SignInManager = serviceProvider.GetService<SignInManager<ApplicationUser>>() ?? throw new ArgumentNullException();
            EmailStore = GetEmailStore();
            TokenHandlerService = serviceProvider.GetService<ITokenHandlerService>() ?? throw new ArgumentNullException();
            UserRefreshTokenRepository = serviceProvider.GetService<IUserRefreshTokenRepository>() ?? throw new ArgumentNullException();
        }

        public async Task<JwtToken> ExternalLoginCallbackAsync(string? remoteError, string? callback, string role)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                throw new Exception($"Error from external provider: {remoteError}");
            }
            var info = await SignInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                throw new Exception();
            }
            var result = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await UserManager.FindByEmailAsync(email);

            if (result.Succeeded)
            {
                if (await UserManager.IsInRoleAsync(user, role))
                {
                    var userRoles = await UserManager.GetRolesAsync(user);
                    return await TokenHandlerService.CreateTokenAsync(user, userRoles);
                }
                else
                {
                    await SignInManager.SignOutAsync();
                    throw new Exception();
                }
            }
            if (result.IsLockedOut)
            {
                throw new Exception();
            }
            else
            {
                if (user == null)
                {
                    await CreateUserAsync(role);
                    user = await UserManager.FindByEmailAsync(email);
                }
                else
                {
                    await UserManager.AddLoginAsync(user, new UserLoginInfo(
                        info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                }

                result = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                
                if (result.Succeeded)
                {
                    var userRoles = await UserManager.GetRolesAsync(user);
                    return await TokenHandlerService.CreateTokenAsync(user, userRoles);
                }

                throw new Exception("Unknown error occured");
            }
        }
        private async Task CreateUserAsync(string role)
        {
            var info = await SignInManager.GetExternalLoginInfoAsync();
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

            if (info == null)
            {
                throw new Exception();
            }

            var user = CreateApplicationUser();

            await UserStore.SetUserNameAsync(user, email, CancellationToken.None);
            await EmailStore.SetEmailAsync(user, email, CancellationToken.None);

            user.FirstName = name ?? string.Empty;
            user.LastName = lastName ?? string.Empty;
            user.Id = Guid.NewGuid().ToString();

            var result = await UserManager.CreateAsync(user);

            if (result.Succeeded)
            {
                result = await UserManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user, role);

                    await SignInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                }
            }
            else
            {
                var str = "";
                foreach (var error in result.Errors)
                {
                    str += $"\r\n {error.Description}";
                }
                throw new Exception(str);
            }
        }
        public IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!UserManager.SupportsUserEmail)
            {
                throw new Exception();
            }
            return (IUserEmailStore<ApplicationUser>)UserStore;
        }
        private ApplicationUser CreateApplicationUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }
        public async Task<JwtToken> RefreshTokenAsync(RefreshTokenInput input)
        {
            string accessToken = input.AccessToken;
            string refreshToken = input.RefreshToken;

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                throw new Exception("Invalid access token or refresh token");

            var claims = TokenHandlerService.GetPrincipalFromExpiredToken(accessToken);
            if (claims == null || !claims.Any())
                throw new Exception("Invalid access token or refresh token");

            var claim = claims.FirstOrDefault(m => m.Type == ClaimTypes.Name);
            if (claim == null)
                throw new Exception("Invalid access token or refresh token");

            string userId = claim.Value;
            var user = await UserManager.FindByIdAsync(userId);

            if (user is null)
                throw new Exception("Invalid access token or refresh token");

            var token = await UserRefreshTokenRepository.SingleOrDefaultAsync(x => x.UserId == new Guid(userId) && x.RefreshToken == refreshToken);

            if (token == null || token.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Invalid access token or refresh token");
            }

            var userRoles = await UserManager.GetRolesAsync(user);
            var newToken = await TokenHandlerService.CreateTokenAsync(user, userRoles);

            return newToken;
        }
    }
}
