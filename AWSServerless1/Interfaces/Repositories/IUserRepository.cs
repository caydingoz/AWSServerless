using AWSServerless1.Models;
using Patika.Framework.Domain.Interfaces.Repository;

namespace AWSServerless1.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User, Guid>
    {
    }
}
