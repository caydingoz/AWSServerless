﻿using AWSServerless_MVC.Models;
using Patika.Framework.Domain.Interfaces.Repository;

namespace AWSServerless_MVC.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User, Guid>
    {
    }
}
