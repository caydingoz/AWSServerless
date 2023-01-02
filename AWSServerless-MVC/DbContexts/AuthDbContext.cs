using AWSServerless_MVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AWSServerless_MVC.DbContexts
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }
    }
}
