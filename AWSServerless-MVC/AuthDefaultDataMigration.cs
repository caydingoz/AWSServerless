using AWSServerless_MVC.Consts;
using AWSServerless_MVC.DbContexts;
using AWSServerless_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Patika.Framework.Shared.Interfaces;

namespace AWSServerless_MVC
{
    internal class AuthDefaultDataMigration : IMigrationStep
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly IUserStore<ApplicationUser> UserStore;
        private readonly IUserEmailStore<ApplicationUser> EmailStore;

        public AuthDefaultDataMigration(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            UserManager = serviceProvider.GetService<UserManager<ApplicationUser>>() ?? throw new Exception();
            UserStore = serviceProvider.GetService<IUserStore<ApplicationUser>>() ?? throw new Exception();
            EmailStore = GetEmailStore();
        }

        public async Task EnsureMigrationAsync()
        {
            try
            {
                var context = ServiceProvider.GetRequiredService<AuthDbContext>();
                await context.Database.MigrateAsync();

                await CreateRoleAsync(RoleConsts.USER_ROLE);
                await CreateRoleAsync(RoleConsts.ADMIN_ROLE);

                await AddAdministrator();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task CreateRoleAsync(string role)
        {
            var manager = ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roleExists = await manager.RoleExistsAsync(role);
            if (!roleExists)
            {
                var newRole = new IdentityRole(role);
                await manager.CreateAsync(newRole);
            }
        }
        private async Task AddAdministrator()
        {
            string name = "Admin_Panel";
            string password = "naBqn07j18hxTZf*cdR~+t7ue1p]!9kf";

            if (await UserManager.FindByEmailAsync($"{name}@patika.com") is not null)
                return;

            var user = CreateUser();
            await UserStore.SetUserNameAsync(user, name, CancellationToken.None);
            await EmailStore.SetEmailAsync(user, $"{name}@patika.com", CancellationToken.None);
            user.PhoneNumber = name;
            var result = await UserManager.CreateAsync(user, password);

            if (!result.Succeeded)
                throw new Exception();

            ApplicationUser adminUser = await UserManager.FindByIdAsync(user.Id);

            if (!await UserManager.IsInRoleAsync(adminUser, RoleConsts.ADMIN_ROLE))
                await UserManager.AddToRoleAsync(adminUser, RoleConsts.ADMIN_ROLE);
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!UserManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)UserStore;
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'.");
            }
        }
    }
}
