using AWSServerless1.Interfaces.Repositories;
using AWSServerless1.Models;
using Microsoft.EntityFrameworkCore;
using Patika.Framework.Domain.Services;

namespace AWSServerless1.Repositories
{
    public class UserRepository : GenericRepository<User, ApplicationDbContext, Guid>, IUserRepository
    {
        public UserRepository(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override ApplicationDbContext GetContext() => new(DbOptions);

        protected override IQueryable<User> GetDbSetWithIncludes(DbContext ctx) => ctx.Set<User>();
    }
}
