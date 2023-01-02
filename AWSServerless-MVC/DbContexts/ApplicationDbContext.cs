using AWSServerless_MVC.Models;
using Microsoft.EntityFrameworkCore;
using Patika.Framework.Domain.Services;

namespace AWSServerless_MVC.DbContexts
{
    public class ApplicationDbContext : DbContextWithUnitOfWork<ApplicationDbContext>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<User> Users { get; set; }
    }
}
