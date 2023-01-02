using AWSServerless_MVC.DbContexts;
using AWSServerless_MVC.Interfaces.Repositories;
using AWSServerless_MVC.Models;
using Microsoft.EntityFrameworkCore;
using Patika.Framework.Domain.Services;

namespace AWSServerless_MVC.Repositories
{
    public class UserRefreshTokenRepository : GenericRepository<UserRefreshToken, AuthDbContext, Guid>, IUserRefreshTokenRepository
    {
        public UserRefreshTokenRepository(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }
        protected override AuthDbContext GetContext() => new(DbOptions);

        protected override IQueryable<UserRefreshToken> GetDbSetWithIncludes(DbContext ctx) => ctx.Set<UserRefreshToken>();
    }
}
